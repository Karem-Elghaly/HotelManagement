
using FluentValidation;

namespace HotelManagement.Application.Availability;

public class AvailabilityQueryValidator : AbstractValidator<AvailabilityQuery>
{
    private const int MaxNights = 30;

    public AvailabilityQueryValidator()
    {
        RuleFor(x => x.CheckIn)
            .Must(d => d >= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Check-in date cannot be in the past.");

        RuleFor(x => x.CheckOut)
            .GreaterThan(x => x.CheckIn)
            .WithMessage("Check-out must be after check-in.");

        RuleFor(x => x.CheckOut)
            .Must((q, checkOut) => checkOut.DayNumber - q.CheckIn.DayNumber <= MaxNights)
            .WithMessage($"Maximum stay is {MaxNights} nights.");

        RuleFor(x => x.Adults).InclusiveBetween(1, 10);
        RuleFor(x => x.Children).InclusiveBetween(0, 10);
    }
}