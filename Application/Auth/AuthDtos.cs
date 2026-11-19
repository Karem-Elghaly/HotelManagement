
namespace HotelManagement.Application.Auth;

public record RegisterRequest(string FirstName, string LastName, string Email, string Password);

public record LoginRequest(string Email, string Password);

public record RefreshTokenRequest(string RefreshToken);

public record UserInfo(string Id, string Email, string FirstName, string LastName, IReadOnlyList<string> Roles);

public record AuthResponse(
    string AccessToken,            
    DateTime AccessTokenExpiresAt,    
    string RefreshToken,              
    UserInfo User);