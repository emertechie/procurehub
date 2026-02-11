using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProcureHub.Common;
using ProcureHub.Constants;
using ProcureHub.Models;

namespace ProcureHub.Data;

public sealed class DataSeeder
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(
        ApplicationDbContext dbContext,
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IConfiguration configuration,
        ILogger<DataSeeder> logger)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedAsync(bool? onlySeedRolesAndAdminUser = false)
    {
        // Kinda hacky. For integration tests
        if (onlySeedRolesAndAdminUser == true)
        {
            await SeedRolesAsync();
            await SeedAdminUserAsync();
        }
        else
        {
            await SeedRolesAsync();
            await SeedDepartmentsAsync();
            await SeedUsersAsync();
            await SeedCategoriesAsync();
            await SeedPurchaseRequestsAsync();
        }
    }

    private async Task SeedRolesAsync()
    {
        var roles = new[] { RoleNames.Admin, RoleNames.Requester, RoleNames.Approver };

        foreach (var roleName in roles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                _logger.LogWarning("Creating role '{RoleName}'", roleName);
                await _roleManager.CreateAsync(new Role(roleName));
            }
        }
    }

    private async Task SeedDepartmentsAsync()
    {
        if (_dbContext.Departments.Any())
        {
            return;
        }

        var departmentNames = new[]
        {
            "Engineering",
            "Marketing",
            "Sales",
            "HR",
            "Finance",
            "Operations"
        };

        var now = DateTime.UtcNow;
        foreach (var name in departmentNames)
        {
            _dbContext.Departments.Add(new Department
            {
                Id = Guid.NewGuid(),
                Name = name,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        await _dbContext.SaveChangesAsync();
        _logger.LogWarning("Seeded {Count} departments", departmentNames.Length);
    }

    private async Task SeedAdminUserAsync()
    {
        var adminEmail = _configuration.GetRequiredString("AdminUserEmail");
        await SeedUserAsync(adminEmail);
    }

    private async Task SeedUsersAsync()
    {
        var testUsers = GetTestSeedUsers();
        foreach (var userSection in testUsers)
        {
            await SeedUserAsync(userSection.Key);
        }
    }

    /// <summary>
    /// Gets SeedUsers configuration entries that start with "test-".
    /// In test environments, this filters to only test users (e.g., test-requester@example.com)
    /// and excludes app users (e.g., requester@example.com) that may be merged from base config.
    /// In production, if no test- users exist, returns all SeedUsers.
    /// </summary>
    private IEnumerable<IConfigurationSection> GetTestSeedUsers()
    {
        var seedUsersSection = _configuration.GetSection("SeedUsers");
        var allUsers = seedUsersSection.GetChildren().ToList();

        var testUsers = allUsers.Where(u => u.Key.StartsWith("test-", StringComparison.OrdinalIgnoreCase)).ToList();

        // If test users exist, use them exclusively. Otherwise fall back to all users (production case).
        return testUsers.Any() ? testUsers : allUsers;
    }

    private async Task SeedUserAsync(string email)
    {
        var configKey = $"SeedUsers:{email}";

        // Check if user config exists
        var userSection = _configuration.GetSection(configKey);
        if (!userSection.Exists())
        {
            _logger.LogWarning("Skipping user '{Email}' - no config found", email);
            return;
        }

        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var user = new User
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = _configuration.GetRequiredString($"{configKey}:FirstName"),
            LastName = _configuration.GetRequiredString($"{configKey}:LastName"),
            EnabledAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        // Assign department if specified
        var departmentName = _configuration[$"{configKey}:Department"];
        if (departmentName != null)
        {
            var department = _dbContext.Departments.FirstOrDefault(d => d.Name == departmentName);
            if (department != null)
            {
                user.DepartmentId = department.Id;
            }
        }

        var password = _configuration.GetRequiredString($"{configKey}:Password");
        var rolesSection = _configuration.GetSection($"{configKey}:Roles");
        var roles = rolesSection.GetChildren().Select(c => c.Value).OfType<string>().ToArray();

        _logger.LogWarning("Creating user with email '{Email}' and roles '{Roles}'", email, string.Join(", ", roles));

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new Exception(
                $"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        foreach (var roleName in roles)
        {
            await _userManager.AddToRoleAsync(user, roleName);
        }
    }

    private async Task SeedCategoriesAsync()
    {
        if (_dbContext.Categories.Any())
        {
            return;
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
            _dbContext.Categories.Add(new Category
            {
                Id = Guid.NewGuid(),
                Name = name,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        await _dbContext.SaveChangesAsync();
        _logger.LogWarning("Seeded {Count} categories", categoryNames.Length);
    }

    private async Task SeedPurchaseRequestsAsync()
    {
        if (_dbContext.PurchaseRequests.Any())
        {
            return;
        }

        // Lookup requester and approver emails from SeedUsers config
        var userEmails = GetTestSeedUsers().Select(c => c.Key).ToList();

        var requesterEmail = userEmails.FirstOrDefault(e => e.Contains("requester", StringComparison.OrdinalIgnoreCase));
        var approverEmail = userEmails.FirstOrDefault(e => e.Contains("approver", StringComparison.OrdinalIgnoreCase));

        if (requesterEmail == null || approverEmail == null)
        {
            _logger.LogWarning("Cannot seed purchase requests - requester or approver email not found in SeedUsers config");
            return;
        }

        var requester = await _userManager.FindByEmailAsync(requesterEmail);
        var approver = await _userManager.FindByEmailAsync(approverEmail);

        if (requester == null || approver == null)
        {
            _logger.LogWarning("Cannot seed purchase requests - requester or approver not found");
            return;
        }

        var engineeringDept = _dbContext.Departments.FirstOrDefault(d => d.Name == "Engineering");
        var marketingDept = _dbContext.Departments.FirstOrDefault(d => d.Name == "Marketing");
        var itEquipmentCategory = _dbContext.Categories.FirstOrDefault(c => c.Name == "IT Equipment");
        var softwareCategory = _dbContext.Categories.FirstOrDefault(c => c.Name == "Software");
        var marketingCategory = _dbContext.Categories.FirstOrDefault(c => c.Name == "Marketing");

        if (engineeringDept == null || marketingDept == null
            || itEquipmentCategory == null || softwareCategory == null || marketingCategory == null)
        {
            _logger.LogWarning("Cannot seed purchase requests - required departments or categories not found");
            return;
        }

        var now = DateTime.UtcNow;
        var requests = new[]
        {
            // Draft request
            new PurchaseRequest
            {
                Id = Guid.NewGuid(),
                RequestNumber = "PR-2025-001",
                Title = "New Development Laptops",
                Description = "Purchase 5 new laptops for the development team",
                EstimatedAmount = 7500.00m,
                BusinessJustification = "Current laptops are 4 years old and causing productivity issues",
                CategoryId = itEquipmentCategory.Id,
                DepartmentId = engineeringDept.Id,
                RequesterId = requester.Id,
                Status = PurchaseRequestStatus.Draft,
                CreatedAt = now.AddDays(-5),
                UpdatedAt = now.AddDays(-5)
            },
            
            // Pending request
            new PurchaseRequest
            {
                Id = Guid.NewGuid(),
                RequestNumber = "PR-2025-002",
                Title = "Software Licenses - JetBrains Suite",
                Description = "Annual renewal of JetBrains licenses for development team",
                EstimatedAmount = 3200.00m,
                BusinessJustification = "Essential IDE tools for development work",
                CategoryId = softwareCategory.Id,
                DepartmentId = engineeringDept.Id,
                RequesterId = requester.Id,
                Status = PurchaseRequestStatus.Pending,
                SubmittedAt = now.AddDays(-2),
                CreatedAt = now.AddDays(-3),
                UpdatedAt = now.AddDays(-2)
            },
            
            // Approved request
            new PurchaseRequest
            {
                Id = Guid.NewGuid(),
                RequestNumber = "PR-2025-003",
                Title = "Marketing Campaign Materials",
                Description = "Printed materials and promotional items for Q1 campaign",
                EstimatedAmount = 2500.00m,
                BusinessJustification = "Required for upcoming product launch",
                CategoryId = marketingCategory.Id,
                DepartmentId = marketingDept.Id,
                RequesterId = requester.Id,
                Status = PurchaseRequestStatus.Approved,
                SubmittedAt = now.AddDays(-10),
                ReviewedAt = now.AddDays(-8),
                ReviewedById = approver.Id,
                CreatedAt = now.AddDays(-12),
                UpdatedAt = now.AddDays(-8)
            },
            
            // Rejected request
            new PurchaseRequest
            {
                Id = Guid.NewGuid(),
                RequestNumber = "PR-2025-004",
                Title = "Premium Office Chairs",
                Description = "Ergonomic office chairs for engineering team",
                EstimatedAmount = 4500.00m,
                BusinessJustification = "Improve team comfort and productivity",
                CategoryId = itEquipmentCategory.Id,
                DepartmentId = engineeringDept.Id,
                RequesterId = requester.Id,
                Status = PurchaseRequestStatus.Rejected,
                SubmittedAt = now.AddDays(-7),
                ReviewedAt = now.AddDays(-6),
                ReviewedById = approver.Id,
                CreatedAt = now.AddDays(-8),
                UpdatedAt = now.AddDays(-6)
            }
        };

        _dbContext.PurchaseRequests.AddRange(requests);
        await _dbContext.SaveChangesAsync();
        _logger.LogWarning("Seeded {Count} purchase requests", requests.Length);
    }
}
