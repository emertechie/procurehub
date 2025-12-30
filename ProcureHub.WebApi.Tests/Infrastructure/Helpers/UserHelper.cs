using System.Net.Http.Json;
using ProcureHub.Features.Roles;
using ProcureHub.Features.Users;
using ProcureHub.WebApi.Responses;
using ProcureHub.WebApi.Tests.Infrastructure.Helpers;

public static class UserHelper
{
    public static async Task<string> CreateUserAsync(
        HttpClient httpClient,
        string userEmail,
        string userPassword,
        string[]? roleNames = null,
        Guid? departmentId = null
    )
    {
        // Create user
        var createUserRequest = new CreateUser.Request(userEmail, userPassword, "Some", "User");
        var createUserResp = await httpClient.PostAsync("/users", JsonContent.Create(createUserRequest));
        var createdUser = await createUserResp.ReadJsonAsync<EntityCreatedResponse<string>>();

        // Get roles
        var rolesResp = await httpClient.GetAsync("/roles");
        var roles = await rolesResp.ReadJsonAsync<DataResponse<List<QueryRoles.Role>>>();

        foreach (var roleName in roleNames ?? [])
        {
            var role = roles.Data.First(r => r.Name == roleName);

            // Assign role
            var assignRoleReq = new AssignRole.Request(createdUser.Id, role.Id);
            await httpClient.PostAsync($"/users/{createdUser.Id}/roles", JsonContent.Create(assignRoleReq));
        }

        if (departmentId.HasValue)
        {
            // Assign department
            var assignDeptReq = new { Id = createdUser.Id, DepartmentId = departmentId.Value };
            await httpClient.PatchAsync($"/users/{createdUser.Id}/department", JsonContent.Create(assignDeptReq));
        }

        return createdUser.Id;
    }
}
