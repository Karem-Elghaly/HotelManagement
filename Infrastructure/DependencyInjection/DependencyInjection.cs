
using System.Security.Claims;
using System.Text;
using HotelManagement.Application.Auth;
using HotelManagement.Application.Availability;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Reports;
using HotelManagement.Application.Reviews;
using HotelManagement.Application.Staff;
using HotelManagement.Infrastructure.BackgroundJobs;
using HotelManagement.Infrastructure.Identity;
using HotelManagement.Infrastructure.Persistence;
using HotelManagement.Infrastructure.Persistence.Queries;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace HotelManagement.Infrastructure;

public static class DependencyInjection
{
  
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
   
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

     
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

     
        var jwt = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();
        if (jwt is null || string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
            throw new InvalidOperationException("Jwt:Key must be set and at least 32 characters. Use user-secrets.");

  
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.User.RequireUniqueEmail = true;

            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;

            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

       
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.NameIdentifier
                };
            });

        services.AddAuthorization();

    
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IReservationLocker, ReservationLocker>();

   
        services.AddScoped<IRoomAvailabilityQuery, RoomAvailabilityQuery>();
        services.AddScoped<IReviewSummaryQuery, ReviewSummaryQuery>();
        services.AddScoped<IReportsQuery, ReportsQuery>();


        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IStaffService, StaffService>();

       
        services.AddHostedService<PendingReservationExpiryService>();
        services.AddHostedService<RefreshTokenCleanupService>();

        return services;
    }
}