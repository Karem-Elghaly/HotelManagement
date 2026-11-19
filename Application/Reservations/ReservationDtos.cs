
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Reservations;

public record CreateReservationRequest(
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int Adults,
    int Children,
    int RoomTypeId,
    int? RoomId,
    int? CustomerId,
    string? SpecialRequests);

public record ReservationResponse(
    int Id,
    string ConfirmationCode,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int Nights,
    int Adults,
    int Children,
    ReservationStatus Status,
    decimal TotalPrice,
    string? SpecialRequests,
    DateTime? CancelledAt,
    int RoomId,
    string RoomNumber,
    string RoomTypeName,
    int CustomerId,
    string CustomerName);

public class ReservationQuery
{
    public string? Search { get; set; }             
    public ReservationStatus? Status { get; set; }
    public DateOnly? CheckInFrom { get; set; }       
    public DateOnly? CheckInTo { get; set; }         
    public int? RoomId { get; set; }
    public int? CustomerId { get; set; }          
    public string? SortBy { get; set; }
    public bool Descending { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}