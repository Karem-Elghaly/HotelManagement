// Entities/Reservation.cs
using HotelManagement.Domain.Common;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Domain.Entities;

public class Reservation : BaseEntity
{

    public string ConfirmationCode { get; set; } = string.Empty;

  
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }   

    public int Adults { get; set; }
    public int Children { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;


    public decimal TotalPrice { get; set; }

    public string? SpecialRequests { get; set; }
    public DateTime? CancelledAt { get; set; }

    public int RoomId { get; set; }
    public Room Room { get; set; } = null!;

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

   
    public int Nights => CheckOutDate.DayNumber - CheckInDate.DayNumber;
}