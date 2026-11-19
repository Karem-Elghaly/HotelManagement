
namespace HotelManagement.Application.Reports;


public interface IReportsQuery
{
  
    Task<int> CountSellableRoomsAsync(CancellationToken ct = default);

   
    Task<IReadOnlyList<StayRow>> GetStaysAsync(DateOnly from, DateOnly to, CancellationToken ct = default);

    Task<IReadOnlyList<PaymentAggregateRow>> GetPaymentAggregatesAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<IReadOnlyList<ExtraAggregateRow>> GetExtrasAggregatesAsync(DateOnly from, DateOnly to, CancellationToken ct = default);

    Task<DashboardResponse> GetDashboardAsync(DateOnly today, CancellationToken ct = default);
}