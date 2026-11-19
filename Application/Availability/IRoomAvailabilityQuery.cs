
using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.Availability;

public interface IRoomAvailabilityQuery
{

    Task<IReadOnlyList<Room>> GetAvailableRoomsAsync(
        DateOnly checkIn, DateOnly checkOut, int adults, int children,
        int? roomTypeId, CancellationToken ct = default);

    Task<bool> IsRoomAvailableAsync(
        int roomId, DateOnly checkIn, DateOnly checkOut,
        int? excludeReservationId = null, CancellationToken ct = default);

   
    Task<bool> HasReservationsAsync(int roomId, DateOnly from, DateOnly until, CancellationToken ct = default);


    Task LockRoomAsync(int roomId, CancellationToken ct = default);
}