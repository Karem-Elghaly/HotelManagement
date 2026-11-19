
using HotelManagement.Domain.Common;

namespace HotelManagement.Domain.Entities;

public class Hotel : BaseEntity
{
   
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

   
    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

 
    public string Phone { get; set; } = string.Empty;

  
    public string Email { get; set; } = string.Empty;

    
    public int StarRating { get; set; }

    
    public TimeOnly CheckInTime { get; set; } = new(14, 0);

   
    public TimeOnly CheckOutTime { get; set; } = new(12, 0);

  
    public int FreeCancellationHours { get; set; } = 48;
}