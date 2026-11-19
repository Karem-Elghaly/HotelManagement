
namespace HotelManagement.Application.Reports;

public interface IReportService
{
    Task<OccupancyReport> GetOccupancyAsync(ReportRangeQuery query, CancellationToken ct = default);
    Task<RevenueReport> GetRevenueAsync(ReportRangeQuery query, CancellationToken ct = default);
    Task<DashboardResponse> GetDashboardAsync(CancellationToken ct = default);
}