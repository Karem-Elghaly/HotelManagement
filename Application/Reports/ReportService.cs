
using FluentValidation;

namespace HotelManagement.Application.Reports;

public class ReportService : IReportService
{
    private readonly IReportsQuery _query;
    private readonly IValidator<ReportRangeQuery> _validator;

    public ReportService(IReportsQuery query, IValidator<ReportRangeQuery> validator)
    {
        _query = query;
        _validator = validator;
    }

    public async Task<OccupancyReport> GetOccupancyAsync(ReportRangeQuery query, CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(query, ct);

        var rooms = await _query.CountSellableRoomsAsync(ct);
        var stays = await _query.GetStaysAsync(query.From, query.To, ct);

        return OccupancyCalculator.Calculate(stays, rooms, query.From, query.To);
    }

    public async Task<RevenueReport> GetRevenueAsync(ReportRangeQuery query, CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(query, ct);

        var payments = await _query.GetPaymentAggregatesAsync(query.From, query.To, ct);
        var extras = await _query.GetExtrasAggregatesAsync(query.From, query.To, ct);

        return RevenueCalculator.Calculate(query.From, query.To, payments, extras);
    }


    public Task<DashboardResponse> GetDashboardAsync(CancellationToken ct = default)
        => _query.GetDashboardAsync(DateOnly.FromDateTime(DateTime.UtcNow), ct);
}