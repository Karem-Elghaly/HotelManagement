
namespace HotelManagement.Application.Billing;

public interface IBillingService
{
    Task<BillResponse> GetBillAsync(int reservationId, CancellationToken ct = default);

    Task<BillResponse> AddExtraAsync(int reservationId, AddExtraRequest request, CancellationToken ct = default);
    Task<BillResponse> RemoveExtraAsync(int reservationId, int extraId, CancellationToken ct = default);

    Task<BillResponse> PayAsync(int reservationId, CreatePaymentRequest request, CancellationToken ct = default);
    Task<BillResponse> RefundAsync(int reservationId, RefundRequest request, CancellationToken ct = default);

   
    Task<decimal> GetBalanceAsync(int reservationId, CancellationToken ct = default);
}