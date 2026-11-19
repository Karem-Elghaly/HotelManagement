// Rooms/RoomDtos.cs
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Rooms;

// بيانات الأوضة في Create و Update. لاحظ إن الـ Status مش هنا
// لأن الحالة ليها endpoint مخصوص وقواعد خاصة بيها
public record SaveRoomRequest(
    string Number,
    int Floor,
    int RoomTypeId,
    string? Notes);

// طلب تغيير الحالة بس
public record UpdateRoomStatusRequest(RoomStatus Status);

public record RoomResponse(
    int Id,
    string Number,
    int Floor,
    RoomStatus Status,
    string? Notes,
    int RoomTypeId,
    string RoomTypeName);   // بنرجعه عشان العميل ميحتاجش request تاني يجيب اسم النوع

// ?search=10&status=Available&floor=1&roomTypeId=2&sortBy=floor&descending=true
public class RoomQuery
{
    public string? Search { get; set; }       // بحث في رقم الأوضة
    public RoomStatus? Status { get; set; }   // nullable: لو null يعني "كل الحالات"
    public int? Floor { get; set; }
    public int? RoomTypeId { get; set; }
    public string? SortBy { get; set; }
    public bool Descending { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}