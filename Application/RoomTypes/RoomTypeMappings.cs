
using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.RoomTypes;

public static class RoomTypeMappings
{

    public static RoomTypeResponse ToResponse(this RoomType e) => new(
        e.Id, e.Name, e.Description, e.BasePricePerNight,
        e.MaxAdults, e.MaxChildren, e.AreaInSquareMeters);

 
    public static RoomType ToEntity(this SaveRoomTypeRequest r) => new()
    {
        Name = r.Name.Trim(), 
        Description = r.Description?.Trim(),
        BasePricePerNight = r.BasePricePerNight,
        MaxAdults = r.MaxAdults,
        MaxChildren = r.MaxChildren,
        AreaInSquareMeters = r.AreaInSquareMeters
    };

    public static void ApplyTo(this SaveRoomTypeRequest r, RoomType e)
    {
        e.Name = r.Name.Trim();
        e.Description = r.Description?.Trim();
        e.BasePricePerNight = r.BasePricePerNight;
        e.MaxAdults = r.MaxAdults;
        e.MaxChildren = r.MaxChildren;
        e.AreaInSquareMeters = r.AreaInSquareMeters;
    }
}