using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Application.Common;
using ProcureHub.Application.Common.Authorization;
using ProcureHub.Application.Constants;
using ProcureHub.Domain.Entities;

namespace ProcureHub.Application.Features.Roles;

public static class QueryRoles
{
    [AuthorizeRequest(RoleNames.Admin)]
    public record Request : IRequest<RoleInfo[]>;

    public record RoleInfo(string Id, string Name);

    public class Handler(RoleManager<Role> roleManager)
        : IRequestHandler<Request, RoleInfo[]>
    {
        public async Task<RoleInfo[]> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var roles = await roleManager.Roles
                .OrderBy(r => r.Name)
                .Select(r => new RoleInfo(r.Id, r.Name!))
                .ToArrayAsync(cancellationToken);

            return roles;
        }
    }
}
