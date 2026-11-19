// Rooms/RoomService.cs
using System.Linq.Expressions;
using FluentValidation;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Entities;
using HotelManagement.Domain.Enums;
using HotelManagement.Domain.Exceptions;

namespace HotelManagement.Application.Rooms;

public class RoomService : IRoomService
{
    private readonly IRepository<Room> _rooms;
    private readonly IRepository<RoomType> _roomTypes;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<SaveRoomRequest> _saveValidator;
    private readonly IValidator<UpdateRoomStatusRequest> _statusValidator;
    private readonly IRepository<Reservation> _reservations;

    public RoomService(
        IRepository<Room> rooms,
        IRepository<RoomType> roomTypes,
         IRepository<Reservation> reservations,   // الجديد
        IUnitOfWork uow,
        IValidator<SaveRoomRequest> saveValidator,
        IValidator<UpdateRoomStatusRequest> statusValidator)
    {
        _rooms = rooms;
        _roomTypes = roomTypes;
        _uow = uow;
        _saveValidator = saveValidator;
        _reservations = reservations;
        _statusValidator = statusValidator;
    }

    public async Task<PagedResult<RoomResponse>> GetAllAsync(RoomQuery query, CancellationToken ct = default)
    {
        var pageNumber = Math.Max(query.PageNumber, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);

        var term = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();
        var status = query.Status;
        var floor = query.Floor;
        var roomTypeId = query.RoomTypeId;

        Expression<Func<Room, bool>> filter = r =>
            (term == null || r.Number.Contains(term)) &&
            (status == null || r.Status == status) &&
            (floor == null || r.Floor == floor) &&
            (roomTypeId == null || r.RoomTypeId == roomTypeId);

        // 6 arguments: filter, orderBy, pageNumber, pageSize, ct, include
        var (items, total) = await _rooms.GetPagedAsync(
            filter, GetOrderBy(query), pageNumber, pageSize, ct, r => r.RoomType);

        return new PagedResult<RoomResponse>(
            items.Select(e => e.ToResponse()).ToList(), total, pageNumber, pageSize);
    }

    public async Task<RoomResponse> GetByIdAsync(int id, CancellationToken ct = default)
        => (await GetOrThrowAsync(id, ct)).ToResponse();

    public async Task<RoomResponse> CreateAsync(SaveRoomRequest request, CancellationToken ct = default)
    {
        await _saveValidator.ValidateAndThrowAsync(request, ct);

        var roomType = await _roomTypes.GetByIdAsync(request.RoomTypeId, ct)
            ?? throw new NotFoundException(nameof(RoomType), request.RoomTypeId);

        var number = request.Number.Trim();
        if (await _rooms.ExistsAsync(r => r.Number == number, ct))
            throw new DomainException($"Room '{number}' already exists.");

        var entity = request.ToEntity();
        entity.RoomType = roomType;

        await _rooms.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        return entity.ToResponse();
    }

    public async Task<RoomResponse> UpdateAsync(int id, SaveRoomRequest request, CancellationToken ct = default)
    {
        await _saveValidator.ValidateAndThrowAsync(request, ct);

        var entity = await GetOrThrowAsync(id, ct);

        var roomType = await _roomTypes.GetByIdAsync(request.RoomTypeId, ct)
            ?? throw new NotFoundException(nameof(RoomType), request.RoomTypeId);

        var number = request.Number.Trim();
        if (await _rooms.ExistsAsync(r => r.Number == number && r.Id != id, ct))
            throw new DomainException($"Room '{number}' already exists.");

        request.ApplyTo(entity);
        entity.RoomType = roomType;

        _rooms.Update(entity);
        await _uow.SaveChangesAsync(ct);

        return entity.ToResponse();
    }

    public async Task<RoomResponse> UpdateStatusAsync(int id, UpdateRoomStatusRequest request, CancellationToken ct = default)
    {
        await _statusValidator.ValidateAndThrowAsync(request, ct);

        var entity = await GetOrThrowAsync(id, ct);

        if (entity.Status == RoomStatus.Occupied)
            throw new DomainException("Cannot change the status of an occupied room. Check the guest out first.");

        if (request.Status == RoomStatus.Occupied)
            throw new DomainException("A room becomes occupied only through check-in.");

        entity.Status = request.Status;
        _rooms.Update(entity);
        await _uow.SaveChangesAsync(ct);

        return entity.ToResponse();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await GetOrThrowAsync(id, ct);

        if (entity.Status == RoomStatus.Occupied)
            throw new DomainException("Cannot delete an occupied room.");

        // أي حجز (حتى القديم) بيمنع المسح: لو الأوضة اتعملها Soft Delete، حجوزاتها بتختفي من القوايم
        // الحل الصح للأوضة اللي مش هتتستخدم: نخليها OutOfService
        if (await _reservations.ExistsAsync(r => r.RoomId == id, ct))
            throw new DomainException("This room has reservations history and cannot be deleted. Set it to OutOfService instead.");

        _rooms.Remove(entity);
        await _uow.SaveChangesAsync(ct);
    }

    private async Task<Room> GetOrThrowAsync(int id, CancellationToken ct)
        => await _rooms.GetByIdAsync(id, ct, r => r.RoomType)
           ?? throw new NotFoundException(nameof(Room), id);

    private static Func<IQueryable<Room>, IOrderedQueryable<Room>> GetOrderBy(RoomQuery q)
    {
        var desc = q.Descending;
        return (q.SortBy?.ToLowerInvariant()) switch
        {
            "floor" => src => desc
                ? src.OrderByDescending(x => x.Floor).ThenBy(x => x.Number)
                : src.OrderBy(x => x.Floor).ThenBy(x => x.Number),
            "status" => src => desc
                ? src.OrderByDescending(x => x.Status).ThenBy(x => x.Number)
                : src.OrderBy(x => x.Status).ThenBy(x => x.Number),
            _ => src => desc
                ? src.OrderByDescending(x => x.Number)
                : src.OrderBy(x => x.Number)
        };
    }
}