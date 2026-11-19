
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.RoomTypes;

public interface IRoomTypeService
{
    Task<PagedResult<RoomTypeResponse>> GetAllAsync(RoomTypeQuery query, CancellationToken ct = default);
    Task<RoomTypeResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<RoomTypeResponse> CreateAsync(SaveRoomTypeRequest request, CancellationToken ct = default);
    Task<RoomTypeResponse> UpdateAsync(int id, SaveRoomTypeRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}