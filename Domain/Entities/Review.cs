
using HotelManagement.Domain.Common;

namespace HotelManagement.Domain.Entities;


public class Review : BaseEntity
{
 
    public int ReservationId { get; set; }
    public Reservation Reservation { get; set; } = null!;

   
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

  
    public int Rating { get; set; }

    public string? Comment { get; set; }


    public bool IsHidden { get; set; }


    public string? ManagerReply { get; set; }
    public DateTime? RepliedAt { get; set; }
}