
using FluentValidation;

namespace HotelManagement.Application.Pricing;

public class SaveSeasonalRateRequestValidator : AbstractValidator<SaveSeasonalRateRequest>
{
    public SaveSeasonalRateRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RoomTypeId).GreaterThan(0).When(x => x.RoomTypeId.HasValue);

        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate)
            .WithMessage("EndDate must be after StartDate.");
     
        RuleFor(x => x.EndDate)
            .Must((r, end) => end.DayNumber - r.StartDate.DayNumber <= 3650)
            .WithMessage("A rate cannot span more than 10 years.");

       
        RuleFor(x => x.Multiplier)
            .InclusiveBetween(0.1m, 5m)
            .Must(m => decimal.Round(m, 2) == m).WithMessage("Multiplier can have at most 2 decimal places.");

        RuleFor(x => x.Priority).InclusiveBetween(0, 100);
    }
}

public class QuoteQueryValidator : AbstractValidator<QuoteQuery>
{
    public QuoteQueryValidator()
    {
        RuleFor(x => x.RoomTypeId).GreaterThan(0);
        RuleFor(x => x.CheckOut).GreaterThan(x => x.CheckIn).WithMessage("Check-out must be after check-in.");
        RuleFor(x => x.CheckOut)
            .Must((q, co) => co.DayNumber - q.CheckIn.DayNumber <= 30)
            .WithMessage("Maximum stay is 30 nights.");
    }
}