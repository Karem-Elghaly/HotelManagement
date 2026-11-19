
using HotelManagement.Domain.Common;

namespace HotelManagement.Domain.Entities;

public class RoomType : BaseEntity
{

    public string Name { get; set; } = string.Empty;

  
    public string? Description { get; set; }


    public decimal BasePricePerNight { get; set; }


    public int MaxAdults { get; set; }


    public int MaxChildren { get; set; }


    public int AreaInSquareMeters { get; set; }

    
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}