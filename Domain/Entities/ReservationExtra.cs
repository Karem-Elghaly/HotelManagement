// Entities/ReservationExtra.cs
using HotelManagement.Domain.Common;

namespace HotelManagement.Domain.Entities;

public class ReservationExtra : BaseEntity
{
    public int ReservationId { get; set; }
    public Reservation Reservation { get; set; } = null!;


    public int ExtraServiceId { get; set; }

   
    public string ServiceName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal Total => UnitPrice * Quantity;
}