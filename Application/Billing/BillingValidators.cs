// Billing/BillingValidators.cs
using FluentValidation;

namespace HotelManagement.Application.Billing;

public class AddExtraRequestValidator : AbstractValidator<AddExtraRequest>
{
    public AddExtraRequestValidator()
    {
        RuleFor(x => x.ExtraServiceId).GreaterThan(0);
  
        RuleFor(x => x.Quantity).InclusiveBetween(1, 20);
    }
}

public class CreatePaymentRequestValidator : AbstractValidator<CreatePaymentRequest>
{
    public CreatePaymentRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .Must(a => decimal.Round(a, 2) == a).WithMessage("Amount can have at most 2 decimal places.");
        RuleFor(x => x.Method).IsInEnum();
        RuleFor(x => x.Notes).MaximumLength(300);
    }
}

public class RefundRequestValidator : AbstractValidator<RefundRequest>
{
    public RefundRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .Must(a => decimal.Round(a, 2) == a).WithMessage("Amount can have at most 2 decimal places.");
        RuleFor(x => x.Method).IsInEnum();
        RuleFor(x => x.Notes).MaximumLength(300);
    }
}