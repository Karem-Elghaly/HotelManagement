
using FluentValidation;

namespace HotelManagement.Application.Maintenance;

public class CreateMaintenanceRequestValidator : AbstractValidator<CreateMaintenanceRequest>
{
    private const int MaxBlockDays = 180;

    public CreateMaintenanceRequestValidator()
    {
        RuleFor(x => x.RoomId).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.Priority).IsInEnum();

 
        RuleFor(x => x.BlockedFrom)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("BlockedFrom is required when the room is blocked.")
            .Must(d => d!.Value >= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("BlockedFrom cannot be in the past.")
            .When(x => x.BlocksRoom);

        RuleFor(x => x.BlockedUntil)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("BlockedUntil is required when the room is blocked.")
            .Must((r, until) => r.BlockedFrom is null || until!.Value > r.BlockedFrom.Value)
            .WithMessage("BlockedUntil must be after BlockedFrom.")
            .Must((r, until) => r.BlockedFrom is null ||
                                until!.Value.DayNumber - r.BlockedFrom.Value.DayNumber <= MaxBlockDays)
            .WithMessage($"A room cannot be blocked for more than {MaxBlockDays} days.")
            .When(x => x.BlocksRoom);


        RuleFor(x => x.BlockedFrom).Null()
            .WithMessage("BlockedFrom must be empty when the room is not blocked.")
            .When(x => !x.BlocksRoom);
        RuleFor(x => x.BlockedUntil).Null()
            .WithMessage("BlockedUntil must be empty when the room is not blocked.")
            .When(x => !x.BlocksRoom);
    }
}

public class ResolveMaintenanceRequestValidator : AbstractValidator<ResolveMaintenanceRequest>
{
    public ResolveMaintenanceRequestValidator()
    {
        RuleFor(x => x.ResolutionNotes).MaximumLength(1000);
    }
}