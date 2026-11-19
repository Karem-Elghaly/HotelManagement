
using HotelManagement.Domain.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HotelManagement.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var config = services.GetRequiredService<IConfiguration>();

        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        
        await EnsureUserAsync(userManager, config["SeedAdmin:Email"], config["SeedAdmin:Password"],
            "System", "Admin", Roles.Admin);
        await EnsureUserAsync(userManager, config["SeedStaff:Receptionist:Email"], config["SeedStaff:Receptionist:Password"],
            "Front", "Desk", Roles.Receptionist);
        await EnsureUserAsync(userManager, config["SeedStaff:Housekeeper1:Email"], config["SeedStaff:Housekeeper1:Password"],
            "House", "Keeper1", Roles.Housekeeper);
        await EnsureUserAsync(userManager, config["SeedStaff:Housekeeper2:Email"], config["SeedStaff:Housekeeper2:Password"],
            "House", "Keeper2", Roles.Housekeeper);
    }


    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager, string? email, string? password,
        string firstName, string lastName, string role)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;
        if (await userManager.FindByEmailAsync(email) is not null) return;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName
        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(user, role);
    }

}