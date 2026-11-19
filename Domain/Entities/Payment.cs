
using HotelManagement.Domain.Common;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Domain.Entities;


public class Payment : BaseEntity
{
    
    public string Reference { get; set; } = string.Empty;

    
    public decimal Amount { get; set; }

    public PaymentType Type { get; set; }
    public PaymentMethod Method { get; set; }
    public string? Notes { get; set; }

  
    public string? CreatedByUserId { get; set; }

    public int ReservationId { get; set; }
    public Reservation Reservation { get; set; } = null!;
}