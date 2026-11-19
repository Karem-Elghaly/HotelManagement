
using HotelManagement.Domain.Entities;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Billing;

public record AddExtraRequest(int ExtraServiceId, int Quantity);

public record CreatePaymentRequest(decimal Amount, PaymentMethod Method, string? Notes);


public record RefundRequest(decimal Amount, PaymentMethod Method, string? Notes);

public record ReservationExtraResponse(
    int Id, int ExtraServiceId, string ServiceName, int Quantity, decimal UnitPrice, decimal Total);

public record PaymentResponse(
    int Id, string Reference, PaymentType Type, PaymentMethod Method,
    decimal Amount, string? Notes, DateTime CreatedAt);

public record BillResponse(
    int ReservationId,
    string ConfirmationCode,
    ReservationStatus Status,
    decimal RoomTotal,       
    decimal ExtrasTotal,    
    decimal GrandTotal,    
    decimal AmountDue,       
    decimal TotalPaid,
    decimal TotalRefunded,
    decimal NetPaid,         
    decimal Balance,         
    IReadOnlyList<ReservationExtraResponse> Extras,
    IReadOnlyList<PaymentResponse> Payments);

public static class BillingMappings
{
    public static ReservationExtraResponse ToResponse(this ReservationExtra e)
        => new(e.Id, e.ExtraServiceId, e.ServiceName, e.Quantity, e.UnitPrice, e.Total);

    public static PaymentResponse ToResponse(this Payment p)
        => new(p.Id, p.Reference, p.Type, p.Method, p.Amount, p.Notes, p.CreatedAt);
}