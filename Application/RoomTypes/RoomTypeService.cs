
using System.Linq.Expressions;
using FluentValidation;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Entities;
using HotelManagement.Domain.Exceptions;


namespace HotelManagement.Application.RoomTypes;

public class RoomTypeService : IRoomTypeService
{
    private readonly IRepository<RoomType> _roomTypes;
    private readonly IRepository<Room> _rooms;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<SaveRoomTypeRequest> _validator;


    public RoomTypeService(
        IRepository<RoomType> roomTypes,
        IRepository<Room> rooms,
        IUnitOfWork uow,
        IValidator<SaveRoomTypeRequest> validator)
    {
        _roomTypes = roomTypes;
        _rooms = rooms;
        _uow = uow;
        _validator = validator;
    }

    public async Task<PagedResult<RoomTypeResponse>> GetAllAsync(RoomTypeQuery query, CancellationToken ct = default)
    {
       
        var pageNumber = Math.Max(query.PageNumber, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);

 
        Expression<Func<RoomType, bool>>? filter = null;
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            filter = rt => rt.Name.Contains(term);
        }

        var (items, total) = await _roomTypes.GetPagedAsync(
            filter, GetOrderBy(query), pageNumber, pageSize, ct);

       
        return new PagedResult<RoomTypeResponse>(
            items.Select(e => e.ToResponse()).ToList(), total, pageNumber, pageSize);
    }

    public async Task<RoomTypeResponse> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await GetOrThrowAsync(id, ct);
        return entity.ToResponse();
    }

    public async Task<RoomTypeResponse> CreateAsync(SaveRoomTypeRequest request, CancellationToken ct = default)
    {
        
        await _validator.ValidateAndThrowAsync(request, ct);

      
        var name = request.Name.Trim();
        if (await _roomTypes.ExistsAsync(rt => rt.Name == name, ct))
            throw new DomainException($"Room type '{name}' already exists.");

        var entity = request.ToEntity();
        await _roomTypes.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);  

        return entity.ToResponse();
    }

    public async Task<RoomTypeResponse> UpdateAsync(int id, SaveRoomTypeRequest request, CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(request, ct);

        var entity = await GetOrThrowAsync(id, ct);

      
        var name = request.Name.Trim();
        if (await _roomTypes.ExistsAsync(rt => rt.Name == name && rt.Id != id, ct))
            throw new DomainException($"Room type '{name}' already exists.");

        request.ApplyTo(entity);
        _roomTypes.Update(entity);
        await _uow.SaveChangesAsync(ct);  

        return entity.ToResponse();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await GetOrThrowAsync(id, ct);

       
        if (await _rooms.ExistsAsync(r => r.RoomTypeId == id, ct))
            throw new DomainException("Cannot delete a room type that still has rooms.");

        _roomTypes.Remove(entity);       
        await _uow.SaveChangesAsync(ct);
    }


    private async Task<RoomType> GetOrThrowAsync(int id, CancellationToken ct)
        => await _roomTypes.GetByIdAsync(id, ct)
           ?? throw new NotFoundException(nameof(RoomType), id);

    
    private static Func<IQueryable<RoomType>, IOrderedQueryable<RoomType>> GetOrderBy(RoomTypeQuery q)
    {
        var desc = q.Descending;
        return (q.SortBy?.ToLowerInvariant()) switch
        {
            "price" => src => desc ? src.OrderByDescending(x => x.BasePricePerNight) : src.OrderBy(x => x.BasePricePerNight),
            "area" => src => desc ? src.OrderByDescending(x => x.AreaInSquareMeters) : src.OrderBy(x => x.AreaInSquareMeters),
            _ => src => desc ? src.OrderByDescending(x => x.Name) : src.OrderBy(x => x.Name)
        };
    }
}