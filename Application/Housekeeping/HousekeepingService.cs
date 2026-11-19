// Housekeeping/HousekeepingService.cs
using System.Linq.Expressions;
using FluentValidation;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Constants;
using HotelManagement.Domain.Entities;
using HotelManagement.Domain.Enums;
using HotelManagement.Domain.Exceptions;

namespace HotelManagement.Application.Housekeeping;

public class HousekeepingService : IHousekeepingService
{
    // بنجيب الأوضة مع كل مهمة (JOIN) عشان الـ Response فيه رقم الأوضة والدور
    private static readonly Expression<Func<HousekeepingTask, object>>[] Includes = [t => t.Room];

    private readonly IRepository<HousekeepingTask> _tasks;
    private readonly IRepository<Room> _rooms;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<CreateHousekeepingTaskRequest> _createValidator;
    private readonly IValidator<CompleteHousekeepingTaskRequest> _completeValidator;

    public HousekeepingService(
        IRepository<HousekeepingTask> tasks,
        IRepository<Room> rooms,
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        IValidator<CreateHousekeepingTaskRequest> createValidator,
        IValidator<CompleteHousekeepingTaskRequest> completeValidator)
    {
        _tasks = tasks;
        _rooms = rooms;
        _uow = uow;
        _currentUser = currentUser;
        _createValidator = createValidator;
        _completeValidator = completeValidator;
    }

    private bool IsFrontDesk =>
        _currentUser.IsInRole(Roles.Admin) ||
        _currentUser.IsInRole(Roles.Manager) ||
        _currentUser.IsInRole(Roles.Receptionist);

   

    public async Task<PagedResult<HousekeepingTaskResponse>> GetAllAsync(
        HousekeepingTaskQuery query, CancellationToken ct = default)
    {
        var pageNumber = Math.Max(query.PageNumber, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);

     
        var userId = query.Mine
            ? _currentUser.UserId ?? throw new UnauthorizedException("You must be logged in.")
            : null;

   
        var status = query.Status;
        var open = query.Open;
        var mine = query.Mine;
        var unassigned = query.Unassigned;
        var roomId = query.RoomId;

        Expression<Func<HousekeepingTask, bool>> filter = t =>
            (status == null || t.Status == status) &&
            (!open || HousekeepingTask.OpenStatuses.Contains(t.Status)) &&
            (!mine || t.AssignedToUserId == userId) &&
            (!unassigned || t.AssignedToUserId == null) &&
            (roomId == null || t.RoomId == roomId);

        var (items, total) = await _tasks.GetPagedAsync(
            filter, GetOrderBy(query), pageNumber, pageSize, ct, Includes);

        return new PagedResult<HousekeepingTaskResponse>(
            items.Select(t => t.ToResponse()).ToList(), total, pageNumber, pageSize);
    }

    public async Task<HousekeepingTaskResponse> GetByIdAsync(int id, CancellationToken ct = default)
        => (await GetOrThrowAsync(id, ct)).ToResponse();



    public async Task<HousekeepingTaskResponse> CreateAsync(
        CreateHousekeepingTaskRequest request, CancellationToken ct = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, ct);

        var room = await _rooms.GetByIdAsync(request.RoomId, ct)
            ?? throw new NotFoundException(nameof(Room), request.RoomId);

        if (await _tasks.ExistsAsync(t =>
                t.RoomId == room.Id &&
                t.Type == request.Type &&
                HousekeepingTask.OpenStatuses.Contains(t.Status), ct))
            throw new DomainException($"Room {room.Number} already has an open {request.Type} task.");

        var task = new HousekeepingTask
        {
            RoomId = room.Id,
            Type = request.Type,
            Notes = request.Notes?.Trim(),
            CreatedByUserId = _currentUser.UserId
        };

        await _tasks.AddAsync(task, ct);
        await _uow.SaveChangesAsync(ct);

        
        return (await GetOrThrowAsync(task.Id, ct)).ToResponse();
    }

    
    public async Task<HousekeepingTaskResponse> StartAsync(int id, CancellationToken ct = default)
    {
        var task = await GetOrThrowAsync(id, ct);

        if (task.Status != HousekeepingTaskStatus.Pending)
            throw new DomainException($"Cannot start a task that is {task.Status}.");

        task.Status = HousekeepingTaskStatus.InProgress;
        task.AssignedToUserId = _currentUser.UserId;
        task.StartedAt = DateTime.UtcNow;

       
        await _uow.SaveChangesAsync(ct);
        return task.ToResponse();
    }

    public async Task<HousekeepingTaskResponse> CompleteAsync(
        int id, CompleteHousekeepingTaskRequest request, CancellationToken ct = default)
    {
        await _completeValidator.ValidateAndThrowAsync(request, ct);

        var task = await GetOrThrowAsync(id, ct);

        if (task.Status != HousekeepingTaskStatus.InProgress)
            throw new DomainException($"Cannot complete a task that is {task.Status}. Start it first.");

        if (!IsFrontDesk && task.AssignedToUserId != _currentUser.UserId)
            throw new DomainException("This task was started by another staff member.");

        task.Status = HousekeepingTaskStatus.Completed;
        task.CompletedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(request.Notes))
            task.Notes = request.Notes.Trim();

        
        if (task.Room.Status == RoomStatus.Cleaning &&
            !await _tasks.ExistsAsync(t =>
                t.RoomId == task.RoomId &&
                t.Id != task.Id &&
                HousekeepingTask.OpenStatuses.Contains(t.Status), ct))
        {
            task.Room.Status = RoomStatus.Available;
        }

       
        await _uow.SaveChangesAsync(ct);
        return task.ToResponse();
    }

    public async Task<HousekeepingTaskResponse> CancelAsync(int id, CancellationToken ct = default)
    {
        var task = await GetOrThrowAsync(id, ct);

        if (!HousekeepingTask.OpenStatuses.Contains(task.Status))
            throw new DomainException($"Cannot cancel a task that is {task.Status}.");

      
        task.Status = HousekeepingTaskStatus.Cancelled;
        await _uow.SaveChangesAsync(ct);
        return task.ToResponse();
    }

   

    private async Task<HousekeepingTask> GetOrThrowAsync(int id, CancellationToken ct)
        => await _tasks.GetByIdAsync(id, ct, Includes)
           ?? throw new NotFoundException(nameof(HousekeepingTask), id);

    private static Func<IQueryable<HousekeepingTask>, IOrderedQueryable<HousekeepingTask>> GetOrderBy(
        HousekeepingTaskQuery q)
    {
        var desc = q.Descending;

        return (q.SortBy?.ToLowerInvariant()) switch
        {
           
            "floor" => src => desc
                ? src.OrderByDescending(x => x.Room.Floor).ThenBy(x => x.Room.Number).ThenBy(x => x.Id)
                : src.OrderBy(x => x.Room.Floor).ThenBy(x => x.Room.Number).ThenBy(x => x.Id),
            _ => src => desc
                ? src.OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Id)
                : src.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id)
        };
    }
}