using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ProcureHub.Common;
using ProcureHub.Infrastructure;
using ProcureHub.Models;

namespace ProcureHub.Features.Roles;

public static class AssignRole
{
    public record Request(string UserId, string RoleId);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(r => r.UserId).NotEmpty();
            RuleFor(r => r.RoleId).NotEmpty();
        }
    }

    public class Handler(
        UserManager<User> userManager,
        RoleManager<ApplicationRole> roleManager,
        ILogger<Handler> logger)
        : IRequestHandler<Request, Result>
    {
        public async Task<Result> HandleAsync(Request request, CancellationToken token)
        {
            var user = await userManager.FindByIdAsync(request.UserId);
            if (user is null)
            {
                return Result.Failure(Error.NotFound("User.NotFound", "User not found"));
            }

            var role = await roleManager.FindByIdAsync(request.RoleId);
            if (role is null)
            {
                return Result.Failure(Error.NotFound("Role.NotFound", "Role not found"));
            }

            var result = await userManager.AddToRoleAsync(user, role.Name!);
            if (!result.Succeeded)
            {
                logger.LogWarning("Failed to assign role {RoleId} to user {UserId}. Errors: {Errors}",
                    request.RoleId, request.UserId, string.Join(", ", result.Errors.Select(e => e.Description)));
                return Result.Failure(RoleAssignmentFailed(result.Errors));
            }

            logger.LogInformation("Assigned role {RoleId} to user {UserId}", request.RoleId, request.UserId);
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
