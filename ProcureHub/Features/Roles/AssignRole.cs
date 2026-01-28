using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ProcureHub.Common;
using ProcureHub.Infrastructure;
using ProcureHub.Models;

namespace ProcureHub.Features.Roles;

public static class AssignRole
{
    public record Command(string UserId, string RoleId);

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(r => r.UserId).NotEmpty();
            RuleFor(r => r.RoleId).NotEmpty();
        }
    }

    public class Handler(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        ILogger<Handler> logger)
        : ICommandHandler<Command, Result>
    {
        public async Task<Result> HandleAsync(Command command, CancellationToken token)
        {
            var user = await userManager.FindByIdAsync(command.UserId);
            if (user is null)
            {
                return Result.Failure(Error.NotFound("User.NotFound", "User not found"));
            }

            var role = await roleManager.FindByIdAsync(command.RoleId);
            if (role is null)
            {
                return Result.Failure(Error.NotFound("Role.NotFound", "Role not found"));
            }

            var result = await userManager.AddToRoleAsync(user, role.Name!);
            if (!result.Succeeded)
            {
                logger.LogWarning("Failed to assign role {RoleId} to user {UserId}. Errors: {Errors}",
                    command.RoleId, command.UserId, string.Join(", ", result.Errors.Select(e => e.Description)));
                return Result.Failure(RoleAssignmentFailed(result.Errors));
            }

            logger.LogInformation("Assigned role {RoleId} to user {UserId}", command.RoleId, command.UserId);
            return Result.Success();
        }

        private static Error RoleAssignmentFailed(IEnumerable<IdentityError> identityErrors)
        {
            var errors = new Dictionary<string, string[]>
            {
                ["Role"] = identityErrors.Select(e => e.Description).ToArray()
            };
            return Error.Validation("Role.AssignmentFailed", "Failed to assign role", errors);
        }
    }
}
