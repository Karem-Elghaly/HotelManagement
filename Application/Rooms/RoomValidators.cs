// Rooms/RoomValidators.cs
using FluentValidation;

namespace HotelManagement.Application.Rooms;

public class SaveRoomRequestValidator : AbstractValidator<SaveRoomRequest>
{
    public SaveRoomRequestValidator()
    {
        // نفس حد الداتابيز (20 حرف)
        RuleFor(x => x.Number).NotEmpty().MaximumLength(20);

        // من -2 لأن ممكن يبقى فيه بدروم أو جراج
        RuleFor(x => x.Floor).InclusiveBetween(-2, 100);

        // Id الصح بيبدأ من 1
        RuleFor(x => x.RoomTypeId).GreaterThan(0);

        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public class UpdateRoomStatusRequestValidator : AbstractValidator<UpdateRoomStatusRequest>
{
    public UpdateRoomStatusRequestValidator()
    {
        // IsInEnum: بيتأكد إن الرقم ده فعلًا قيمة موجودة في الـ Enum (مش 99 مثلًا)
        RuleFor(x => x.Status).IsInEnum();
    }
}