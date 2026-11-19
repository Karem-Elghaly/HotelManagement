
namespace HotelManagement.Application.Common.Interfaces;


public interface IReservationLocker
{
    Task LockAsync(int reservationId, CancellationToken ct = default);
}