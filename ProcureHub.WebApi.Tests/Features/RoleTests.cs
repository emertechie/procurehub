using System.Net;
using System.Net.Http.Json;
using ProcureHub.Features.Roles;
using ProcureHub.Features.Users;
using ProcureHub.WebApi.Responses;
using ProcureHub.WebApi.Tests.Infrastructure;
using ProcureHub.WebApi.Tests.Infrastructure.BaseTestTypes;
using ProcureHub.WebApi.Tests.Infrastructure.Xunit;

namespace ProcureHub.WebApi.Tests.Features;

[Collection("ApiTestHost")]
public class RoleTestsWithSharedDb(ApiTestHostFixture hostFixture, ITestOutputHelper testOutputHelper)
    : HttpClientBase(hostFixture, testOutputHelper), IClassFixture<ResetDatabaseFixture>
{
    [Fact]
    public async Task Test_validation_for_all_endpoints()
    {
        await LoginAsAdminAsync();

        await TestHelper.RunTestsForAllAsync<AllRoleEndpoints>(configure =>
        {
            configure.QueryRoles = TestQueryRolesEndpoint;
            configure.AssignRole = TestAssignRoleEndpoint;
            configure.RemoveRole = TestRemoveRoleEndpoint;
        });

        Task TestQueryRolesEndpoint()
        {
            // No params to validate
            return Task.CompletedTask;
        }

        async Task TestAssignRoleEndpoint()
        {
            // No user ID
            var reqNoUserId = new AssignRole.Request("", "role-1");
            var respNoUserId = await HttpClient.PostAsync("/users/user-1/roles", JsonContent.Create(reqNoUserId));
            await respNoUserId.AssertValidationProblemAsync(
                errors: new Dictionary<string, string[]> { ["UserId"] = ["'User Id' must not be empty."] });

            // No role ID
            var reqNoRoleId = new AssignRole.Request("user-1", "");
            var respNoRoleId = await HttpClient.PostAsync("/users/user-1/roles", JsonContent.Create(reqNoRoleId));
            await respNoRoleId.AssertValidationProblemAsync(
                errors: new Dictionary<string, string[]> { ["RoleId"] = ["'Role Id' must not be empty."] });

            // User ID doesn't match query
            var reqUser1Id = new AssignRole.Request("user-1", "role-1");
            var respMismatch = await HttpClient.PostAsync("/users/user-2/roles", JsonContent.Create(reqUser1Id));
            await respMismatch.AssertProblemDetailsAsync(
                HttpStatusCode.BadRequest,
                "Route ID mismatch",
                "Route ID does not match request ID",
                "POST /users/user-2/roles");
        }

        Task TestRemoveRoleEndpoint()
        {
            // Nothing to validate in path params (IDs)
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Ensure_authentication_required_for_all_endpoints()
    {
        // Note: Not logging in to test unauth access

        await TestHelper.RunTestsForAllAsync<AllRoleEndpoints>(configure =>
        {
            configure.QueryRoles = async () =>
            {
                var resp = await HttpClient.GetAsync("/roles");
                Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
            };
            configure.AssignRole = async () =>
            {
                var req = JsonContent.Create(new { RoleId = "role-id" });
                var resp = await HttpClient.PostAsync("/users/user-id/roles", req);
                Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
            };
            configure.RemoveRole = async () =>
            {
                var resp = await HttpClient.DeleteAsync("/users/user-id/roles/role-id");
                Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
            };
        });
    }

    [Fact]
    public async Task Query_roles_returns_all_roles()
    {
        await LoginAsAdminAsync();

        var resp = await HttpClient.GetAsync("/roles");
        resp.EnsureSuccessStatusCode();

        var data = await resp.ReadJsonAsync<DataResponse<List<QueryRoles.Role>>>();
        Assert.NotNull(data.Data);
        Assert.Equal(3, data.Data.Count); // Admin, Requester, Approver
        Assert.Contains(data.Data, r => r.Name == "Admin");
        Assert.Contains(data.Data, r => r.Name == "Requester");
        Assert.Contains(data.Data, r => r.Name == "Approver");
    }
}

[Collection("ApiTestHost")]
public class RoleTests(ApiTestHostFixture hostFixture, ITestOutputHelper testOutputHelper)
    : HttpClientAndDbResetBase(hostFixture, testOutputHelper)
{
    [Fact]
    public async Task Assign_and_remove_roles_workflow()
    {
        await LoginAsAdminAsync();

        // Create a test user
        var createResp = await HttpClient.PostAsync("/users",
            JsonContent.Create(new { Email = "roletest@example.com", Password = "Test1234!", FirstName = "Role", LastName = "Test" }));
        createResp.EnsureSuccessStatusCode();
        var userId = createResp.Headers.Location!.ToString().Split('/').Last();

        // Get all roles
        var rolesResp = await HttpClient.GetAsync("/roles");
        var roles = await rolesResp.ReadJsonAsync<DataResponse<List<QueryRoles.Role>>>();

        var approverRole = roles.Data.First(r => r.Name == "Approver");
        var requesterRole = roles.Data.First(r => r.Name == "Requester");

        // Get user and verify no roles initially
        var getUserResp = await HttpClient.GetAsync($"/users/{userId}");
        var userData = await getUserResp.ReadJsonAsync<DataResponse<GetUserById.Response>>();
        Assert.Empty(userData.Data.Roles);

        // Assign 2 roles
        var assignResp1 = await HttpClient.PostAsync($"/users/{userId}/roles",
            JsonContent.Create(new AssignRole.Request(userId, approverRole.Id)));
        Assert.Equal(HttpStatusCode.NoContent, assignResp1.StatusCode);

        var assignResp2 = await HttpClient.PostAsync($"/users/{userId}/roles",
            JsonContent.Create(new AssignRole.Request(userId, requesterRole.Id)));
        Assert.Equal(HttpStatusCode.NoContent, assignResp2.StatusCode);

        // Verify roles assigned
        var getUserResp2 = await HttpClient.GetAsync($"/users/{userId}");
        var userData2 = await getUserResp2.ReadJsonAsync<DataResponse<GetUserById.Response>>();
        Assert.Equal(new[] { "Approver", "Requester" }, userData2.Data.Roles.Order().ToArray());

        // Remove approver role
        var removeResp = await HttpClient.DeleteAsync($"/users/{userId}/roles/{approverRole.Id}");
        Assert.Equal(HttpStatusCode.NoContent, removeResp.StatusCode);

        // Verify role was removed
        var getUserResp3 = await HttpClient.GetAsync($"/users/{userId}");
        var userData3 = await getUserResp3.ReadJsonAsync<DataResponse<GetUserById.Response>>();
        Assert.Equal(new[] { "Requester" }, userData3.Data.Roles.Order().ToArray());
    }

    [Fact]
    public async Task Assign_role_returns_404_when_user_not_found()
    {
        await LoginAsAdminAsync();

        var rolesResp = await HttpClient.GetAsync("/roles");
        var roles = await rolesResp.ReadJsonAsync<DataResponse<List<QueryRoles.Role>>>();
        var approverRole = roles.Data.First(r => r.Name == "Approver");

        var userId = "nonexistent-user-id";

        var resp = await HttpClient.PostAsync($"/users/{userId}/roles",
            JsonContent.Create(new AssignRole.Request(userId, approverRole.Id)));

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Assign_role_returns_404_when_role_not_found()
    {
        await LoginAsAdminAsync();

        // Create a test user
        var createResp = await HttpClient.PostAsync("/users",
            JsonContent.Create(new { Email = "roletest2@example.com", Password = "Test1234!", FirstName = "Role", LastName = "Test2" }));
        createResp.EnsureSuccessStatusCode();
        var userId = createResp.Headers.Location!.ToString().Split('/').Last();

        var resp = await HttpClient.PostAsync($"/users/{userId}/roles",
            JsonContent.Create(new AssignRole.Request(userId, "nonexistent-role-id")));

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Remove_role_returns_404_when_user_not_found()
    {
        await LoginAsAdminAsync();

        var resp = await HttpClient.DeleteAsync("/users/nonexistent-user-id/roles/some-role-id");

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Remove_role_returns_404_when_role_not_found()
    {
        await LoginAsAdminAsync();

        // Create a test user
        var createResp = await HttpClient.PostAsync("/users",
            JsonContent.Create(new { Email = "roletest3@example.com", Password = "Test1234!", FirstName = "Role", LastName = "Test3" }));
        createResp.EnsureSuccessStatusCode();
        var userId = createResp.Headers.Location!.ToString().Split('/').Last();

        var resp = await HttpClient.DeleteAsync($"/users/{userId}/roles/nonexistent-role-id");

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }
}

internal class AllRoleEndpoints
{
    public Func<Task> QueryRoles { get; set; } = null!;
    public Func<Task> AssignRole { get; set; } = null!;
    public Func<Task> RemoveRole { get; set; } = null!;
}
