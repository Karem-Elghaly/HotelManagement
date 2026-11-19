using HotelManagement.Application.Common.Models;
using HotelManagement.Application.Rooms;

public interface IRoomService
{
    Task<PagedResult<RoomResponse>> GetAllAsync(RoomQuery query, CancellationToken ct = default);
    Task<RoomResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<RoomResponse> CreateAsync(SaveRoomRequest request, CancellationToken ct = default);
    Task<RoomResponse> UpdateAsync(int id, SaveRoomRequest request, CancellationToken ct = default);
    Task<RoomResponse> UpdateStatusAsync(int id, UpdateRoomStatusRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}