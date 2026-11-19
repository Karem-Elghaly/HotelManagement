
using FluentValidation;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Staff;
using HotelManagement.Domain.Constants;
using HotelManagement.Domain.Exceptions;
using HotelManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Infrastructure.Identity;


public class StaffService : IStaffService
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IValidator<CreateStaffRequest> _validator;

    public StaffService(
        UserManager<ApplicationUser> users, AppDbContext db,
        ICurrentUserService current, IValidator<CreateStaffRequest> validator)
    {
        _users = users;
        _db = db;
        _current = current;
        _validator = validator;
    }

    public async Task<StaffResponse> CreateAsync(CreateStaffRequest request, CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(request, ct);
        EnsureCanManage(request.Role);

        var email = request.Email.Trim();
        if (await _users.FindByEmailAsync(email) is not null)
            throw new DomainException("This email is already registered.");

       
        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,   
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim()
        };

        var result = await _users.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new DomainException(string.Join(" ", result.Errors.Select(e => e.Description)));

        var roleResult = await _users.AddToRoleAsync(user, request.Role);
        if (!roleResult.Succeeded)
            throw new DomainException("Could not assign the role.");

        await tx.CommitAsync(ct);
        return ToResponse(user, request.Role);
    }

    public async Task<IReadOnlyList<StaffResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var result = new List<StaffResponse>();

        foreach (var role in Roles.Assignable)
        {
            var inRole = await _users.GetUsersInRoleAsync(role);
            result.AddRange(inRole.Select(u => ToResponse(u, role)));
        }

        return result.OrderBy(s => s.Role).ThenBy(s => s.FullName).ToList();
    }

    public async Task SetActiveAsync(string id, bool active, CancellationToken ct = default)
    {
        var user = await _users.FindByIdAsync(id) ?? throw new NotFoundException("Staff member", id);

        if (user.Id == _current.UserId)
            throw new DomainException("You cannot change your own account status.");

      
        var roles = await _users.GetRolesAsync(user);
        var staffRole = roles.FirstOrDefault(r => Roles.Assignable.Contains(r))
            ?? throw new NotFoundException("Staff member", id);

        EnsureCanManage(staffRole);

        if (!active)
        {
           
            await _users.SetLockoutEnabledAsync(user, true);
            await _users.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

      
            await _db.RefreshTokens
                .Where(t => t.UserId == user.Id && t.RevokedAt == null)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, (DateTime?)DateTime.UtcNow), ct);
        }
        else
        {
            await _users.SetLockoutEndDateAsync(user, null);
            await _users.ResetAccessFailedCountAsync(user);
        }
    }

   
    private void EnsureCanManage(string role)
    {
        if (role == Roles.Manager && !_current.IsInRole(Roles.Admin))
            throw new ForbiddenException("Only an Admin can manage Manager accounts.");
    }

    private static StaffResponse ToResponse(ApplicationUser u, string role) => new(
        u.Id, $"{u.FirstName} {u.LastName}", u.Email!, role,
        IsActive: !(u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow));
}