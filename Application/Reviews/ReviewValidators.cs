
using FluentValidation;

namespace HotelManagement.Application.Reviews;

public class SaveReviewRequestValidator : AbstractValidator<SaveReviewRequest>
{
    public SaveReviewRequestValidator()
    {
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.Comment).MaximumLength(2000);
    }
}

public class ReplyToReviewRequestValidator : AbstractValidator<ReplyToReviewRequest>
{
    public ReplyToReviewRequestValidator()
    {
        RuleFor(x => x.Reply).NotEmpty().MaximumLength(1000);
    }
}