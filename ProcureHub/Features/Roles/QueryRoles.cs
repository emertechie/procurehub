using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Infrastructure;
using ProcureHub.Models;

namespace ProcureHub.Features.Roles;

public static class QueryRoles
{
    public record Request;

    public record Role(string Id, string Name);

    public class Handler(RoleManager<ApplicationRole> roleManager)
        : IRequestHandler<Request, Role[]>
    {
        public async Task<Role[]> HandleAsync(Request request, CancellationToken token)
        {
            var roles = await roleManager.Roles
                .OrderBy(r => r.Name)
                .Select(r => new Role(r.Id, r.Name!))
                .ToArrayAsync(token);

            return roles;
        }
    }
}
