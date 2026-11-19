
namespace HotelManagement.Application.Availability;


public class AvailabilityQuery
{
    public DateOnly CheckIn { get; set; }
    public DateOnly CheckOut { get; set; }
    public int Adults { get; set; } = 1;
    public int Children { get; set; } = 0;
    public int? RoomTypeId { get; set; }   
}


public record RoomTypeAvailabilityResponse(
    int RoomTypeId,
    string Name,
    string? Description,
    int MaxAdults,
    int MaxChildren,
    int AreaInSquareMeters,
    decimal PricePerNight,
    int Nights,
    decimal TotalPrice,
    int AvailableRooms);   


public record AvailableRoomResponse(
    int RoomId,
    string Number,
    int Floor,
    int RoomTypeId,
    string RoomTypeName,
    decimal PricePerNight,
    decimal TotalPrice);