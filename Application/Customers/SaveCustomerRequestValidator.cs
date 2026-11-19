
using FluentValidation;

namespace HotelManagement.Application.Customers;

public class SaveCustomerRequestValidator : AbstractValidator<SaveCustomerRequest>
{
    public SaveCustomerRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(150);

        
        RuleFor(x => x.Phone)
            .Matches(@"^\+?[0-9\s\-()]{6,30}$")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone))
            .WithMessage("Phone number format is invalid.");

        RuleFor(x => x.NationalId).MaximumLength(30);
        RuleFor(x => x.Address).MaximumLength(300);
    }
}