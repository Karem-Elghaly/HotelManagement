
namespace HotelManagement.Application.Availability;

public interface IAvailabilityService
{
    Task<IReadOnlyList<RoomTypeAvailabilityResponse>> SearchAsync(AvailabilityQuery query, CancellationToken ct = default);
    Task<IReadOnlyList<AvailableRoomResponse>> GetAvailableRoomsAsync(AvailabilityQuery query, CancellationToken ct = default);
}