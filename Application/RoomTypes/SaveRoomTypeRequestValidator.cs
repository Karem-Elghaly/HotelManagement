
using FluentValidation;

namespace HotelManagement.Application.RoomTypes;


public class SaveRoomTypeRequestValidator : AbstractValidator<SaveRoomTypeRequest>
{
    public SaveRoomTypeRequestValidator()
    {
     
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);

        RuleFor(x => x.Description).MaximumLength(1000);

   
        RuleFor(x => x.BasePricePerNight).GreaterThan(0);

       
        RuleFor(x => x.MaxAdults).InclusiveBetween(1, 10);
        RuleFor(x => x.MaxChildren).InclusiveBetween(0, 10);

        RuleFor(x => x.AreaInSquareMeters).GreaterThan(0);
    }
}