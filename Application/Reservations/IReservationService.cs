
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Reservations;

public interface IReservationService
{
    Task<ReservationResponse> CreateAsync(CreateReservationRequest request, CancellationToken ct = default);

    Task<PagedResult<ReservationResponse>> GetAllAsync(ReservationQuery query, CancellationToken ct = default); // موظفين
    Task<PagedResult<ReservationResponse>> GetMineAsync(ReservationQuery query, CancellationToken ct = default); // عميل
    Task<ReservationResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ReservationResponse> GetByCodeAsync(string code, CancellationToken ct = default);

    Task<ReservationResponse> ConfirmAsync(int id, CancellationToken ct = default);
    Task<ReservationResponse> CancelAsync(int id, CancellationToken ct = default);
    Task<ReservationResponse> CheckInAsync(int id, CancellationToken ct = default);
    Task<ReservationResponse> CheckOutAsync(int id, CancellationToken ct = default);
    Task<ReservationResponse> MarkNoShowAsync(int id, CancellationToken ct = default);
}