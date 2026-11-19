// Rooms/RoomMappings.cs
using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.Rooms;

public static class RoomMappings
{
    // لازم الـ RoomType يكون محمّل (Include) قبل ما ننادي الـ method دي وإلا هيحصل NullReferenceException
    public static RoomResponse ToResponse(this Room e) => new(
        e.Id, e.Number, e.Floor, e.Status, e.Notes, e.RoomTypeId, e.RoomType.Name);

    // Request -> Entity جديدة. الحالة الافتراضية Available جاية من الـ Entity نفسها
    public static Room ToEntity(this SaveRoomRequest r) => new()
    {
        Number = r.Number.Trim(),
        Floor = r.Floor,
        RoomTypeId = r.RoomTypeId,
        Notes = r.Notes?.Trim()
    };

    // تطبيق الـ Request على Entity موجودة
    public static void ApplyTo(this SaveRoomRequest r, Room e)
    {
        e.Number = r.Number.Trim();
        e.Floor = r.Floor;
        e.RoomTypeId = r.RoomTypeId;
        e.Notes = r.Notes?.Trim();
    }
}