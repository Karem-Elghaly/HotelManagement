
using System.Linq.Expressions;
using FluentValidation;
using HotelManagement.Application.Availability;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Entities;
using HotelManagement.Domain.Enums;
using HotelManagement.Domain.Exceptions;

namespace HotelManagement.Application.Maintenance;

public class MaintenanceService : IMaintenanceService
{
    private static readonly Expression<Func<MaintenanceRequest, object>>[] Includes = [m => m.Room];

    private readonly IRepository<MaintenanceRequest> _requests;
    private readonly IRepository<Room> _rooms;
    private readonly IRepository<HousekeepingTask> _tasks;
    private readonly IRoomAvailabilityQuery _availability;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<CreateMaintenanceRequest> _createValidator;
    private readonly IValidator<ResolveMaintenanceRequest> _resolveValidator;

    public MaintenanceService(
        IRepository<MaintenanceRequest> requests,
        IRepository<Room> rooms,
        IRepository<HousekeepingTask> tasks,
        IRoomAvailabilityQuery availability,
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        IValidator<CreateMaintenanceRequest> createValidator,
        IValidator<ResolveMaintenanceRequest> resolveValidator)
    {
        _requests = requests;
        _rooms = rooms;
        _tasks = tasks;
        _availability = availability;
        _uow = uow;
        _currentUser = currentUser;
        _createValidator = createValidator;
        _resolveValidator = resolveValidator;
    }



    public async Task<PagedResult<MaintenanceResponse>> GetAllAsync(MaintenanceQuery query, CancellationToken ct = default)
    {
        var pageNumber = Math.Max(query.PageNumber, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);

        var term = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();
        var status = query.Status;
        var open = query.Open;
        var priority = query.Priority;
        var roomId = query.RoomId;

        Expression<Func<MaintenanceRequest, bool>> filter = m =>
            (term == null || m.Title.Contains(term) || m.Room.Number.Contains(term)) &&
            (status == null || m.Status == status) &&
            (!open || MaintenanceRequest.ActiveStatuses.Contains(m.Status)) &&
            (priority == null || m.Priority == priority) &&
            (roomId == null || m.RoomId == roomId);

        var (items, total) = await _requests.GetPagedAsync(
            filter, GetOrderBy(query), pageNumber, pageSize, ct, Includes);

        return new PagedResult<MaintenanceResponse>(
            items.Select(m => m.ToResponse()).ToList(), total, pageNumber, pageSize);
    }

    public async Task<MaintenanceResponse> GetByIdAsync(int id, CancellationToken ct = default)
        => (await GetOrThrowAsync(id, ct)).ToResponse();

   

    public async Task<MaintenanceResponse> CreateAsync(CreateMaintenanceRequest request, CancellationToken ct = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, ct);

        var room = await _rooms.GetByIdAsync(request.RoomId, ct)
            ?? throw new NotFoundException(nameof(Room), request.RoomId);

        var entity = new MaintenanceRequest
        {
            RoomId = room.Id,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Priority = request.Priority,
            BlocksRoom = request.BlocksRoom,
            BlockedFrom = request.BlocksRoom ? request.BlockedFrom : null,
            BlockedUntil = request.BlocksRoom ? request.BlockedUntil : null,
            ReportedByUserId = _currentUser.UserId
        };

      
        var newId = await _uow.ExecuteInTransactionAsync(async () =>
        {
            if (request.BlocksRoom)
            {
                
                await _availability.LockRoomAsync(room.Id, ct);

                if (await _availability.HasReservationsAsync(
                        room.Id, request.BlockedFrom!.Value, request.BlockedUntil!.Value, ct))
                    throw new DomainException(
                        $"Room {room.Number} has reservations in this period. Move or cancel them first.");
            }

            await _requests.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);
            return entity.Id;
        }, ct);

        return (await GetOrThrowAsync(newId, ct)).ToResponse();
    }

    public async Task<MaintenanceResponse> StartAsync(int id, CancellationToken ct = default)
    {
        var m = await GetOrThrowAsync(id, ct);

        if (m.Status != MaintenanceStatus.Open)
            throw new DomainException($"Cannot start a request that is {m.Status}.");

        if (m.BlocksRoom)
        {
            if (m.Room.Status == RoomStatus.Occupied)
                throw new DomainException($"Room {m.Room.Number} is occupied. Start the work after check-out.");

            m.Room.Status = RoomStatus.Maintenance;
        }

        m.Status = MaintenanceStatus.InProgress;
        await _uow.SaveChangesAsync(ct);   
        return m.ToResponse();
    }

    public async Task<MaintenanceResponse> ResolveAsync(int id, ResolveMaintenanceRequest request, CancellationToken ct = default)
    {
        await _resolveValidator.ValidateAndThrowAsync(request, ct);

        var m = await GetOrThrowAsync(id, ct);

        if (!MaintenanceRequest.ActiveStatuses.Contains(m.Status))
            throw new DomainException($"Cannot resolve a request that is {m.Status}.");

        m.Status = MaintenanceStatus.Resolved;
        m.ResolvedAt = DateTime.UtcNow;
        m.ResolutionNotes = request.ResolutionNotes?.Trim();

        await ReleaseRoomAsync(m, needsCleaning: true, ct);

        await _uow.SaveChangesAsync(ct);
        return m.ToResponse();
    }

    public async Task<MaintenanceResponse> CancelAsync(int id, CancellationToken ct = default)
    {
        var m = await GetOrThrowAsync(id, ct);

        if (!MaintenanceRequest.ActiveStatuses.Contains(m.Status))
            throw new DomainException($"Cannot cancel a request that is {m.Status}.");

        m.Status = MaintenanceStatus.Cancelled;

        await ReleaseRoomAsync(m, needsCleaning: false, ct);

        await _uow.SaveChangesAsync(ct);
        return m.ToResponse();
    }

   
    private async Task ReleaseRoomAsync(MaintenanceRequest m, bool needsCleaning, CancellationToken ct)
    {
        if (m.Room.Status != RoomStatus.Maintenance) return;

       
        var anotherBlocking = await _requests.ExistsAsync(x =>
            x.RoomId == m.RoomId &&
            x.Id != m.Id &&
            x.BlocksRoom &&
            x.Status == MaintenanceStatus.InProgress, ct);

        if (anotherBlocking) return;

        if (!needsCleaning)
        {
            m.Room.Status = RoomStatus.Available;
            return;
        }

        m.Room.Status = RoomStatus.Cleaning;

   
        if (!await _tasks.ExistsAsync(t =>
                t.RoomId == m.RoomId && HousekeepingTask.OpenStatuses.Contains(t.Status), ct))
        {
            await _tasks.AddAsync(new HousekeepingTask
            {
                RoomId = m.RoomId,
                Type = HousekeepingTaskType.PostMaintenance,
                Notes = $"After maintenance: {m.Title}",
                CreatedByUserId = _currentUser.UserId
            }, ct);
        }
    }

    private async Task<MaintenanceRequest> GetOrThrowAsync(int id, CancellationToken ct)
        => await _requests.GetByIdAsync(id, ct, Includes)
           ?? throw new NotFoundException(nameof(MaintenanceRequest), id);

    private static Func<IQueryable<MaintenanceRequest>, IOrderedQueryable<MaintenanceRequest>> GetOrderBy(MaintenanceQuery q)
    {
        var desc = q.Descending;
      
        return (q.SortBy?.ToLowerInvariant()) switch
        {
            "created" => src => desc
                ? src.OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Id)
                : src.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id),
            _ => src => desc
                ? src.OrderBy(x => x.Priority).ThenBy(x => x.CreatedAt).ThenBy(x => x.Id)
                : src.OrderByDescending(x => x.Priority).ThenBy(x => x.CreatedAt).ThenBy(x => x.Id)
        };
    }
}