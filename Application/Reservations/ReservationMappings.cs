
using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.Reservations;

public static class ReservationMappings
{
    
    public static ReservationResponse ToResponse(this Reservation e) => new(
        e.Id, e.ConfirmationCode, e.CheckInDate, e.CheckOutDate, e.Nights,
        e.Adults, e.Children, e.Status, e.TotalPrice, e.SpecialRequests, e.CancelledAt,
        e.RoomId, e.Room.Number, e.Room.RoomType.Name,
        e.CustomerId, $"{e.Customer.FirstName} {e.Customer.LastName}");
}