
using FluentValidation;

namespace HotelManagement.Application.Reservations;

public class CreateReservationRequestValidator : AbstractValidator<CreateReservationRequest>
{
    private const int MaxNights = 30;

    public CreateReservationRequestValidator()
    {
    
        RuleFor(x => x.CheckInDate)
            .Must(d => d >= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Check-in date cannot be in the past.");

        RuleFor(x => x.CheckOutDate)
            .GreaterThan(x => x.CheckInDate)
            .WithMessage("Check-out must be after check-in.");

        RuleFor(x => x.CheckOutDate)
            .Must((r, checkOut) => checkOut.DayNumber - r.CheckInDate.DayNumber <= MaxNights)
            .WithMessage($"Maximum stay is {MaxNights} nights.");

        RuleFor(x => x.Adults).InclusiveBetween(1, 10);
        RuleFor(x => x.Children).InclusiveBetween(0, 10);
        RuleFor(x => x.RoomTypeId).GreaterThan(0);

   
        RuleFor(x => x.RoomId).GreaterThan(0).When(x => x.RoomId.HasValue);
        RuleFor(x => x.CustomerId).GreaterThan(0).When(x => x.CustomerId.HasValue);

        RuleFor(x => x.SpecialRequests).MaximumLength(500);
    }
}