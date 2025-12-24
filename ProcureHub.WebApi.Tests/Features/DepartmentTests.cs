using System.Net;
using System.Net.Http.Json;
using ProcureHub.Features.Departments;
using ProcureHub.Features.Users;
using ProcureHub.WebApi.Tests.Infrastructure.BaseTestTypes;
using ProcureHub.WebApi.Tests.Infrastructure.Helpers;
using ProcureHub.WebApi.Tests.Infrastructure.Xunit;

namespace ProcureHub.WebApi.Tests.Features;

/// <summary>
/// NOTE: DB is only reset once per class instance, so only use for tests that don't persist state
/// </summary>
[Collection("ApiTestHost")]
public class DepartmentTestsWithSharedDb(
    ApiTestHostFixture hostFixture,
    ITestOutputHelper testOutputHelper,
    UserSetupFixture userSetupFixture)
    : HttpClientBase(hostFixture, testOutputHelper),
        IClassFixture<ResetDatabaseFixture>,
        IClassFixture<UserSetupFixture>,
        IAsyncLifetime
{
    private const string ValidUserEmail = "user1@example.com";
    private const string ValidUserPassword = "Test1234!";

    public async ValueTask InitializeAsync()
    {
        await userSetupFixture.EnsureUserCreated(this, AdminEmail, AdminPassword, ValidUserEmail, ValidUserPassword);
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public static TheoryData<EndpointInfo> GetAllDepartmentEndpoints()
    {
        return new TheoryData<EndpointInfo>
        {
            new EndpointInfo("/departments", "POST", "CreateDepartment", new EndpointTestOptions { RequiresAdmin = true }),
            new EndpointInfo("/departments", "GET", "GetDepartments"),
            new EndpointInfo("/departments/{id}", "GET", "GetDepartmentById"),
            new EndpointInfo("/departments/{id}", "PUT", "UpdateDepartment", new EndpointTestOptions { RequiresAdmin = true }),
            new EndpointInfo("/departments/{id}", "DELETE", "DeleteDepartment", new EndpointTestOptions { RequiresAdmin = true })
        };
    }

    [Theory]
    [MemberData(nameof(GetAllDepartmentEndpoints))]
    public async Task All_department_endpoints_require_authentication(EndpointInfo endpoint)
    {
        // Note: Not logging in as anyone initially

        const string testId = "123";

        var path = endpoint.Path.Replace("{id}", testId);
        var request = new HttpRequestMessage(new HttpMethod(endpoint.Method), path);

        var resp = await HttpClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Theory]
    [MemberData(nameof(GetAllDepartmentEndpoints))]
    public async Task Department_endpoints_enforce_admin_authorization_correctly(EndpointInfo endpoint)
    {
        // Log in as a regular user, not an admin
        await LoginAsync(ValidUserEmail, ValidUserPassword);

        const string testId = "123";

        var path = endpoint.Path.Replace("{id}", testId);
        var request = new HttpRequestMessage(new HttpMethod(endpoint.Method), path);

        var resp = await HttpClient.SendAsync(request);

        if (endpoint.Options?.RequiresAdmin ?? false)
        {
            Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
        }
        else
        {
            // Should NOT be Forbidden (403), but could be 404 or other response
            Assert.NotEqual(HttpStatusCode.Forbidden, resp.StatusCode);
        }
    }
}

[Collection("ApiTestHost")]
public class DepartmentTests(ApiTestHostFixture hostFixture, ITestOutputHelper testOutputHelper)
    : HttpClientAndDbResetBase(hostFixture, testOutputHelper)
{
    [Fact]
    public async Task Can_create_and_fetch_department()
    {
        await LoginAsAdminAsync();

        // Assert no departments yet
        var departments1 = await HttpClient.GetAsync("/departments")
            .ReadJsonAsync<ListDepartments.Response[]>();
        Assert.Empty(departments1);

        // Create department
        var createDeptReq = new CreateDepartment.Request("New Department");
        var createDeptResp = await HttpClient.PostAsync("/departments", JsonContent.Create(createDeptReq));
        Assert.Equal(HttpStatusCode.Created, createDeptResp.StatusCode);

        // Extract department ID from Location header
        var location = createDeptResp.Headers.Location?.ToString();
        Assert.Matches(@"^/departments/\d+$", location);
        var newDepartmentId = int.Parse(location!.Split('/').Last());

        // Assert department returned in list
        var departments2 = await HttpClient.GetAsync("/departments")
            .ReadJsonAsync<ListDepartments.Response[]>();
        Assert.Equal(
            new ListDepartments.Response[] { new(newDepartmentId, "New Department") },
            departments2);

        // Assert can get department by ID
        var department = await HttpClient.GetAsync($"/departments/{newDepartmentId}")
            .ReadJsonAsync<GetDepartment.Response>();
        Assert.Equal(
            new GetDepartment.Response(newDepartmentId, "New Department"),
            department);
    }

    [Fact]
    public async Task Admin_can_update_department_name()
    {
        await LoginAsAdminAsync();

        // Create department
        var createReq = new CreateDepartment.Request("Marketing");
        var createResp = await HttpClient.PostAsync("/departments", JsonContent.Create(createReq));
        var deptId = int.Parse(createResp.Headers.Location!.ToString().Split('/').Last());

        // Update department name
        var updateReq = new UpdateDepartment.Request(deptId, "Marketing & Sales");
        var updateResp = await HttpClient.PutAsync($"/departments/{deptId}", JsonContent.Create(updateReq));
        Assert.Equal(HttpStatusCode.NoContent, updateResp.StatusCode);

        // Verify update
        var getDeptResp = await HttpClient.GetAsync($"/departments/{deptId}")
            .ReadJsonAsync<GetDepartment.Response>();
        Assert.Equal("Marketing & Sales", getDeptResp.Name);
    }

    [Fact]
    public async Task Update_department_returns_not_found_for_nonexistent_department()
    {
        await LoginAsAdminAsync();

        var updateReq = new UpdateDepartment.Request(99999, "Nonexistent Dept");
        var updateResp = await HttpClient.PutAsync("/departments/99999", JsonContent.Create(updateReq));

        await updateResp.AssertProblemDetailsAsync(
            HttpStatusCode.NotFound,
            "Department not found",
            "NotFound",
            "PUT /departments/99999");
    }

    [Fact]
    public async Task Admin_can_delete_empty_department()
    {
        await LoginAsAdminAsync();

        // Create department
        var createReq = new CreateDepartment.Request("Temporary Dept");
        var createResp = await HttpClient.PostAsync("/departments", JsonContent.Create(createReq));
        var deptId = int.Parse(createResp.Headers.Location!.ToString().Split('/').Last());

        // Verify department exists in list
        var deptsBefore = await HttpClient.GetAsync("/departments")
            .ReadJsonAsync<ListDepartments.Response[]>();
        Assert.Contains(deptsBefore, d => d.Id == deptId);

        // Delete department
        var deleteResp = await HttpClient.DeleteAsync($"/departments/{deptId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResp.StatusCode);

        // Verify deletion - department no longer in list
        var deptsAfter = await HttpClient.GetAsync("/departments")
            .ReadJsonAsync<ListDepartments.Response[]>();
        Assert.DoesNotContain(deptsAfter, d => d.Id == deptId);
    }

    [Fact]
    public async Task Cannot_delete_department_with_active_users()
    {
        await LoginAsAdminAsync();

        // Create department
        var deptReq = new CreateDepartment.Request("Finance");
        var deptResp = await HttpClient.PostAsync("/departments", JsonContent.Create(deptReq));
        var deptId = int.Parse(deptResp.Headers.Location!.ToString().Split('/').Last());

        // Create user
        var userReq = new CreateUser.Request("finance.user@example.com", "Test1234!", "Finance", "User");
        var userResp = await HttpClient.PostAsync("/users", JsonContent.Create(userReq));
        var userId = userResp.Headers.Location!.ToString().Split('/').Last();

        // Assign user to department
        var assignReq = new AssignUserToDepartment.Request(userId, deptId);
        var assignResp = await HttpClient.PatchAsync($"/users/{userId}/department", JsonContent.Create(assignReq));
        Assert.Equal(HttpStatusCode.NoContent, assignResp.StatusCode);

        // Enable user (make them "active")
        var enableResp = await HttpClient.PatchAsync($"/users/{userId}/enable", null);
        Assert.Equal(HttpStatusCode.NoContent, enableResp.StatusCode);

        // Attempt to delete department - should fail with validation error
        var deleteResp = await HttpClient.DeleteAsync($"/departments/{deptId}");

        await deleteResp.AssertProblemDetailsAsync(
            HttpStatusCode.BadRequest,
            "Cannot delete department. It has 1 active user(s). Please reassign users before deleting.",
            "Validation.Error",
            $"DELETE /departments/{deptId}");
    }

    [Fact]
    public async Task Can_delete_department_with_disabled_users()
    {
        await LoginAsAdminAsync();

        // Create department
        var deptReq = new CreateDepartment.Request("HR");
        var deptResp = await HttpClient.PostAsync("/departments", JsonContent.Create(deptReq));
        var deptId = int.Parse(deptResp.Headers.Location!.ToString().Split('/').Last());

        // Create user
        var userReq = new CreateUser.Request("hr.user@example.com", "Test1234!", "HR", "User");
        var userResp = await HttpClient.PostAsync("/users", JsonContent.Create(userReq));
        var userId = userResp.Headers.Location!.ToString().Split('/').Last();

        // Assign user to department
        var assignReq = new AssignUserToDepartment.Request(userId, deptId);
        await HttpClient.PatchAsync($"/users/{userId}/department", JsonContent.Create(assignReq));

        // Enable user
        await HttpClient.PatchAsync($"/users/{userId}/enable", null);

        // Disable user (make them inactive)
        var disableResp = await HttpClient.PatchAsync($"/users/{userId}/disable", null);
        Assert.Equal(HttpStatusCode.NoContent, disableResp.StatusCode);

        // Attempt to delete department - should succeed
        var deleteResp = await HttpClient.DeleteAsync($"/departments/{deptId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResp.StatusCode);
    }

    [Fact]
    public async Task Delete_department_returns_not_found_for_nonexistent_department()
    {
        await LoginAsAdminAsync();

        var deleteResp = await HttpClient.DeleteAsync("/departments/99999");

        await deleteResp.AssertProblemDetailsAsync(
            HttpStatusCode.NotFound,
            "Department not found",
            "NotFound",
            "DELETE /departments/99999");
    }

    [Fact]
    public async Task Update_department_validates_route_id_matches_body_id()
    {
        await LoginAsAdminAsync();

        // Create department
        var createReq = new CreateDepartment.Request("IT");
        var createResp = await HttpClient.PostAsync("/departments", JsonContent.Create(createReq));
        var deptId = int.Parse(createResp.Headers.Location!.ToString().Split('/').Last());

        // Update with mismatched IDs
        var updateReq = new UpdateDepartment.Request(deptId, "IT Department");
        var differentId = deptId + 1;
        var updateResp = await HttpClient.PutAsync($"/departments/{differentId}", JsonContent.Create(updateReq));

        Assert.Equal(HttpStatusCode.BadRequest, updateResp.StatusCode);
    }
}
