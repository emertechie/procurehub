using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ProcureHub.Constants;
using ProcureHub.Models;

namespace ProcureHub.Data;

public sealed class DataSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext dbContext,
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        ILogger<DataSeeder> logger,
        string adminEmail,
        string adminPassword)
    {
        // Seed roles
        await SeedRolesAsync(roleManager, logger);

        // Seed initial admin user
        await SeedAdminUserAsync(dbContext, userManager, logger, adminEmail, adminPassword);

        // Seed categories
        await SeedCategoriesAsync(dbContext, logger);
    }

    private static async Task SeedRolesAsync(
        RoleManager<Role> roleManager,
        ILogger<DataSeeder> logger)
    {
        var roles = new[] { RoleNames.Admin, RoleNames.Requester, RoleNames.Approver };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                logger.LogWarning("Creating role '{RoleName}'", roleName);
                await roleManager.CreateAsync(new Role(roleName));
            }
        }
    }

    private static async Task SeedAdminUserAsync(ApplicationDbContext dbContext,
        UserManager<User> userManager,
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
        var now = DateTime.UtcNow;
        var adminUser = new User
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FirstName = "Admin",
            LastName = "User",
            EnabledAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        logger.LogWarning("Creating default admin user with email '{Email}'", adminEmail);
        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (!result.Succeeded)
        {
            throw new Exception(
                $"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        // Assign Admin role
        await userManager.AddToRoleAsync(adminUser, RoleNames.Admin);
    }

    private static async Task SeedCategoriesAsync(
        ApplicationDbContext dbContext,
        ILogger<DataSeeder> logger)
    {
        if (dbContext.Categories.Any())
        {
            return; // Categories already seeded
        }

        var categoryNames = new[]
        {
            "IT Equipment",
            "IT Infrastructure",
            "Software",
            "Office Equipment",
            "Marketing",
            "Travel",
            "Training",
            "Professional Services",
            "Office Supplies",
            "Other"
        };

        var now = DateTime.UtcNow;
        foreach (var name in categoryNames)
        {
            dbContext.Categories.Add(new Category
            {
                Id = Guid.NewGuid(),
                Name = name,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        await dbContext.SaveChangesAsync();
        logger.LogWarning("Seeded {Count} categories", categoryNames.Length);
    }
}
