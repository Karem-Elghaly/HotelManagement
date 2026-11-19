
using FluentValidation;
using HotelManagement.Domain.Constants;

namespace HotelManagement.Application.Staff;

public record CreateStaffRequest(string FirstName, string LastName, string Email, string Password, string Role);

public record StaffResponse(string Id, string FullName, string Email, string Role, bool IsActive);

public interface IStaffService
{
    Task<StaffResponse> CreateAsync(CreateStaffRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<StaffResponse>> GetAllAsync(CancellationToken ct = default);
    Task SetActiveAsync(string id, bool active, CancellationToken ct = default);
}

public class CreateStaffRequestValidator : AbstractValidator<CreateStaffRequest>
{
    public CreateStaffRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(150);

        RuleFor(x => x.Password)
            .NotEmpty().MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.");

        RuleFor(x => x.Role)
            .Must(r => Roles.Assignable.Contains(r))
            .WithMessage($"Role must be one of: {string.Join(", ", Roles.Assignable)}.");
    }
}