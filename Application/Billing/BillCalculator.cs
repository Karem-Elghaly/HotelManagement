
using HotelManagement.Domain.Entities;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Billing;


internal static class BillCalculator
{
    public static BillResponse Calculate(
        Reservation r, IReadOnlyList<ReservationExtra> extras, IReadOnlyList<Payment> payments)
    {
        var roomTotal = r.TotalPrice;
        var extrasTotal = extras.Sum(e => e.Total);
        var grandTotal = roomTotal + extrasTotal;

        var paid = payments.Where(p => p.Type == PaymentType.Payment).Sum(p => p.Amount);
        var refunded = payments.Where(p => p.Type == PaymentType.Refund).Sum(p => p.Amount);
        var netPaid = paid - refunded;

       
        var amountDue = r.Status == ReservationStatus.Cancelled ? 0m : grandTotal;

        return new BillResponse(
            r.Id, r.ConfirmationCode, r.Status,
            roomTotal, extrasTotal, grandTotal, amountDue,
            paid, refunded, netPaid,
            amountDue - netPaid,
            extras.OrderBy(e => e.Id).Select(e => e.ToResponse()).ToList(),
            payments.OrderBy(p => p.CreatedAt).ThenBy(p => p.Id).Select(p => p.ToResponse()).ToList());
    }
}