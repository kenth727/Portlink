using PortlinkApp.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace PortlinkApp.Core.Data;

public static class AuthDbInitializer
{
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // Seed Roles
        var roles = new[] { "PortOperator", "Viewer" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Seed Demo Users
        await CreateUserIfNotExists(userManager, "admin@portlink.com", "Admin123!", "Port Administrator", "PortOperator");
        await CreateUserIfNotExists(userManager, "viewer@portlink.com", "View123!", "Port Viewer", "Viewer");
    }

    private static async Task CreateUserIfNotExists(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string fullName,
        string role)
    {
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            return; // User already exists
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            EmailConfirmed = true // Auto-confirm for demo users
        };

        var result = await userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }
}
