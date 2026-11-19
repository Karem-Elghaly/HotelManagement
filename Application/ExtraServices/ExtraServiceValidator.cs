
using FluentValidation;

namespace HotelManagement.Application.ExtraServices;

public class SaveExtraServiceRequestValidator : AbstractValidator<SaveExtraServiceRequest>
{
    public SaveExtraServiceRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);

     
        RuleFor(x => x.Price)
            .GreaterThan(0)
            .Must(p => decimal.Round(p, 2) == p).WithMessage("Price can have at most 2 decimal places.");
    }
}