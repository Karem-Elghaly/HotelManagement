
using HotelManagement.Domain.Common;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Domain.Entities;

public class Room : BaseEntity
{

    public string Number { get; set; } = string.Empty;


    public int Floor { get; set; }


    public RoomStatus Status { get; set; } = RoomStatus.Available;

   
    public string? Notes { get; set; }


    public int RoomTypeId { get; set; }


    public RoomType RoomType { get; set; } = null!;
}