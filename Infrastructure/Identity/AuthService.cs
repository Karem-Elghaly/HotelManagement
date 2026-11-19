using System.Security.Cryptography;
using System.Text;
using FluentValidation;
using HotelManagement.Application.Auth;
using HotelManagement.Domain.Constants;
using HotelManagement.Domain.Entities;
using HotelManagement.Domain.Exceptions;
using HotelManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HotelManagement.Infrastructure.Identity;

public class AuthService : IAuthService
{
   
    private const string InvalidCredentials = "Invalid email or password.";

    private readonly UserManager<ApplicationUser> _users;
    private readonly AppDbContext _db;
    private readonly IJwtTokenGenerator _jwt;
    private readonly JwtSettings _settings;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<RefreshTokenRequest> _refreshValidator;

    public AuthService(
        UserManager<ApplicationUser> users, AppDbContext db, IJwtTokenGenerator jwt,
        IOptions<JwtSettings> settings,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        IValidator<RefreshTokenRequest> refreshValidator)
    {
        _users = users; _db = db; _jwt = jwt; _settings = settings.Value;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _refreshValidator = refreshValidator;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        await _registerValidator.ValidateAndThrowAsync(request, ct);

        var email = request.Email.Trim();
        if (await _users.FindByEmailAsync(email) is not null)
            throw new DomainException("This email is already registered.");

        
        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim()
        };

        var result = await _users.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new DomainException(string.Join(" ", result.Errors.Select(e => e.Description)));

     
        var roleResult = await _users.AddToRoleAsync(user, Roles.Customer);
        if (!roleResult.Succeeded)
            throw new DomainException("Could not complete registration. Please try again.");

      
        _db.Customers.Add(new Customer
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = email
        });
        await _db.SaveChangesAsync(ct);

        await tx.CommitAsync(ct);

        return await IssueTokensAsync(user, ct);
    }
    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        await _loginValidator.ValidateAndThrowAsync(request, ct);

        var user = await _users.FindByEmailAsync(request.Email.Trim())
            ?? throw new UnauthorizedException(InvalidCredentials);

        if (await _users.IsLockedOutAsync(user))
            throw new UnauthorizedException("Account is temporarily locked. Try again later.");

        if (!await _users.CheckPasswordAsync(user, request.Password))
        {
            await _users.AccessFailedAsync(user);
            throw new UnauthorizedException(InvalidCredentials);
        }

        await _users.ResetAccessFailedCountAsync(user);
        return await IssueTokensAsync(user, ct);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        await _refreshValidator.ValidateAndThrowAsync(request, ct);

        var hash = Hash(request.RefreshToken);

        var stored = await _db.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == hash, ct)
            ?? throw new UnauthorizedException("Invalid refresh token.");

       
        if (stored.IsRevoked)
        {
            var now = DateTime.UtcNow;
            await _db.RefreshTokens
                .Where(t => t.UserId == stored.UserId && t.RevokedAt == null)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now), ct);

            throw new UnauthorizedException("Invalid refresh token.");
        }

        if (stored.IsExpired)
            throw new UnauthorizedException("Refresh token expired. Please log in again.");

        
        var user = await _users.FindByIdAsync(stored.UserId)
            ?? throw new UnauthorizedException("Invalid refresh token.");

        stored.RevokedAt = DateTime.UtcNow;


        return await IssueTokensAsync(user, ct);
    }

    


    public async Task LogoutAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        await _refreshValidator.ValidateAndThrowAsync(request, ct);

        var hash = Hash(request.RefreshToken);
        var stored = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

       
        if (stored is { IsRevoked: false })
        {
            stored.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
    }

    private async Task<AuthResponse> IssueTokensAsync(ApplicationUser user, CancellationToken ct)
    {
        var roles = (await _users.GetRolesAsync(user)).ToList();
        var (accessToken, expiresAt) = _jwt.CreateAccessToken(user, roles);

        var rawRefresh = _jwt.GenerateRefreshToken();

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = Hash(rawRefresh),  
            ExpiresAt = DateTime.UtcNow.AddDays(_settings.RefreshTokenDays)
        });
        await _db.SaveChangesAsync(ct);

        var info = new UserInfo(user.Id, user.Email!, user.FirstName, user.LastName, roles);
        return new AuthResponse(accessToken, expiresAt, rawRefresh, info);
    }

    private static string Hash(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}