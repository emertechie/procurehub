using Microsoft.AspNetCore.Identity;
using ProcureHub.Application.Constants;
using ProcureHub.Domain.Entities;

namespace ProcureHub.Application.IntegrationTests.Infrastructure.Identity;

public sealed class IdentityTestDataBuilder(
    UserManager<User> userManager,
    RoleManager<Role> roleManager)
{
    public const string ValidPassword = "Password1!";
    public const string AdminEmail = "test-admin@example.com";

    public async Task EnsureRolesAsync(params string[] roleNames)
    {
        var rolesToCreate = roleNames.Length == 0
            ? [RoleNames.Admin, RoleNames.Requester, RoleNames.Approver]
            : roleNames;

        foreach (var roleName in rolesToCreate)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new Role(roleName));
            }
        }
    }

    public async Task<User> EnsureAdminAsync()
    {
        return await EnsureUserAsync(
            email: AdminEmail,
            password: ValidPassword,
            firstName: "Test",
            lastName: "Admin",
            roles: [RoleNames.Admin]);
    }

    public async Task<User> EnsureUserAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        IEnumerable<string>? roles = null,
        Guid? departmentId = null)
    {
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            await EnsureRolesAssignedAsync(existingUser, roles);
            return existingUser;
        }

        var now = DateTime.UtcNow;
        var user = new User
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName,
            DepartmentId = departmentId,
            EnabledAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to create test user '{email}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        await EnsureRolesAssignedAsync(user, roles);
        return user;
    }

    private async Task EnsureRolesAssignedAsync(User user, IEnumerable<string>? roles)
    {
        var roleNames = roles?.Distinct(StringComparer.OrdinalIgnoreCase).ToArray() ?? [];
        if (roleNames.Length == 0)
        {
            return;
        }

        await EnsureRolesAsync(roleNames);

        foreach (var roleName in roleNames)
        {
            if (!await userManager.IsInRoleAsync(user, roleName))
            {
                var addRoleResult = await userManager.AddToRoleAsync(user, roleName);
                if (!addRoleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to add role '{roleName}' to '{user.Email}': {string.Join(", ", addRoleResult.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}
