
using HotelManagement.Domain.Common;

namespace HotelManagement.Domain.Entities;

public class SeasonalRate : BaseEntity
{
    public string Name { get; set; } = string.Empty;   

    
    public int? RoomTypeId { get; set; }

    public DateOnly StartDate { get; set; }   
    public DateOnly EndDate { get; set; }     

    
    public decimal Multiplier { get; set; }

    
    public bool WeekendsOnly { get; set; }

   
    public int Priority { get; set; }

    public bool IsActive { get; set; } = true;
}