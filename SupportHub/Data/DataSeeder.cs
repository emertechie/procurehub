using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SupportHub.Constants;
using SupportHub.Models;

namespace SupportHub.Data;

public sealed class DataSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<DataSeeder> logger,
        string adminEmail,
        string adminPassword)
    {
        // Seed roles
        await SeedRolesAsync(roleManager, logger);

        // Seed initial admin user
        await SeedAdminUserAsync(dbContext, userManager, logger, adminEmail, adminPassword);
    }
    
    private static async Task SeedRolesAsync(
        RoleManager<IdentityRole> roleManager,
        ILogger<DataSeeder> logger)
    {
        var roles = new[] { RoleNames.Admin, RoleNames.Staff };
        
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                logger.LogWarning("Creating role '{RoleName}'", roleName);
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
    
    private static async Task SeedAdminUserAsync(ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        ILogger<DataSeeder> logger,
        string adminEmail,
        string adminPassword)
    {
        // Check if admin user already exists
        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin != null)
        {
            return; // Admin already exists
        }
        
        // Create admin user
        var adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };
        
        logger.LogWarning("Creating default admin user with email '{Email}'", adminEmail);
        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (!result.Succeeded)
        {
            throw new Exception($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
        
        // Assign Admin role
        await userManager.AddToRoleAsync(adminUser, RoleNames.Admin);
        
        // Create linked Staff record
        var staff = new Staff
        {
            UserId = adminUser.Id,
            EnabledAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        dbContext.Staff.Add(staff);
        await dbContext.SaveChangesAsync();
    }
}
