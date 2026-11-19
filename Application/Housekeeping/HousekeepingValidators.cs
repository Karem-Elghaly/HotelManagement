
using FluentValidation;

namespace HotelManagement.Application.Housekeeping;

public class CreateHousekeepingTaskRequestValidator : AbstractValidator<CreateHousekeepingTaskRequest>
{
    public CreateHousekeepingTaskRequestValidator()
    {
        RuleFor(x => x.RoomId).GreaterThan(0);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public class CompleteHousekeepingTaskRequestValidator : AbstractValidator<CompleteHousekeepingTaskRequest>
{
    public CompleteHousekeepingTaskRequestValidator()
    {
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}