
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Reports;

internal static class RevenueCalculator
{
    public static RevenueReport Calculate(
        DateOnly from, DateOnly to,
        IReadOnlyList<PaymentAggregateRow> payments,
        IReadOnlyList<ExtraAggregateRow> extras)
    {
        decimal Sum(IEnumerable<PaymentAggregateRow> rows, PaymentType type)
            => rows.Where(r => r.Type == type).Sum(r => r.Amount);

        var totalPayments = Sum(payments, PaymentType.Payment);
        var totalRefunds = Sum(payments, PaymentType.Refund);

       
        var daily = payments
            .GroupBy(p => p.Date)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var paid = Sum(g, PaymentType.Payment);
                var refunded = Sum(g, PaymentType.Refund);
                return new DailyRevenue(g.Key, paid, refunded, paid - refunded);
            })
            .ToList();

  
        var byMethod = payments
            .GroupBy(p => p.Method)
            .Select(g => new MethodRevenue(g.Key, Sum(g, PaymentType.Payment) - Sum(g, PaymentType.Refund)))
            .OrderByDescending(m => m.Net)
            .ToList();

        var topExtras = extras
            .OrderByDescending(e => e.Revenue)
            .Take(5)
            .Select(e => new ExtraRevenue(e.ServiceName, e.Quantity, e.Revenue))
            .ToList();

        return new RevenueReport(
            from, to, totalPayments, totalRefunds, totalPayments - totalRefunds,
            daily, byMethod, topExtras);
    }
}