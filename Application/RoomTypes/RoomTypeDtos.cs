
namespace HotelManagement.Application.RoomTypes;


public record SaveRoomTypeRequest(
    string Name,
    string? Description,
    decimal BasePricePerNight,
    int MaxAdults,
    int MaxChildren,
    int AreaInSquareMeters);


public record RoomTypeResponse(
    int Id,
    string Name,
    string? Description,
    decimal BasePricePerNight,
    int MaxAdults,
    int MaxChildren,
    int AreaInSquareMeters);


public class RoomTypeQuery
{
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public bool Descending { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}