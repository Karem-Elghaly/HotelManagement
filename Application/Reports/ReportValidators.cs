
using FluentValidation;

namespace HotelManagement.Application.Reports;

public class ReportRangeQueryValidator : AbstractValidator<ReportRangeQuery>
{
    private const int MaxDays = 366;

    public ReportRangeQueryValidator()
    {
        RuleFor(x => x.To)
            .GreaterThan(x => x.From)
            .WithMessage("'to' must be after 'from'.");

     
        RuleFor(x => x.To)
            .Must((q, to) => to.DayNumber - q.From.DayNumber <= MaxDays)
            .WithMessage($"The range cannot exceed {MaxDays} days.");
    }
}