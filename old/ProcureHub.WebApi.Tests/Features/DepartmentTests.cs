using System.Net;
using System.Net.Http.Json;
using ProcureHub.Constants;
using ProcureHub.Features.Departments;
using ProcureHub.Features.Users;
using ProcureHub.WebApi.Responses;
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
    private const string RequesterUserEmail = "user1@example.com";
    private const string RequesterUserPassword = "Test1234!";

    public async ValueTask InitializeAsync()
    {
        await userSetupFixture.EnsureUserCreated(this,
            AdminEmail,
            AdminPassword,
            RequesterUserEmail,
            RequesterUserPassword,
            RoleNames.Requester
        );
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

        var testId = Guid.NewGuid();

        var path = endpoint.Path.Replace("{id}", testId.ToString());
        var request = new HttpRequestMessage(new HttpMethod(endpoint.Method), path);

        var resp = await HttpClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Theory]
    [MemberData(nameof(GetAllDepartmentEndpoints))]
    public async Task Department_endpoints_enforce_admin_authorization_correctly(EndpointInfo endpoint)
    {
        // Log in as a regular user, not an admin
        await LoginAsync(RequesterUserEmail, RequesterUserPassword);

        var testId = Guid.NewGuid();

        var path = endpoint.Path.Replace("{id}", testId.ToString());
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

    #region Endpoint Validation Tests

    [Theory]
    [MemberData(nameof(GetAllDepartmentEndpoints))]
    public void All_department_endpoints_have_validation_tests(EndpointInfo endpoint)
    {
        // Verify test method exists using reflection
        var testMethod = GetType().GetMethod($"Test_{endpoint.Name}_validation");
        Assert.NotNull(testMethod);
    }

    [Fact]
    public async Task Test_CreateDepartment_validation()
    {
        await LoginAsAdminAsync();

        // No name
        var cmdNoName = new CreateDepartment.Command(null!);
        var respNoName = await HttpClient.PostAsync("/departments", JsonContent.Create(cmdNoName));
        await respNoName.AssertValidationProblemAsync(
            errors: new Dictionary<string, string[]> { ["Name"] = ["'Name' must not be empty."] });
    }

    [Fact]
    public async Task Test_GetDepartments_validation()
    {
        // No validation - no parameters
    }

    [Fact]
    public async Task Test_GetDepartmentById_validation()
    {
        // No validation - id comes from route only
    }

    [Fact]
    public async Task Test_UpdateDepartment_validation()
    {
        await LoginAsAdminAsync();

        // No name
        var cmdNoName = new UpdateDepartment.Command(Guid.NewGuid(), null!);
        var respNoName = await HttpClient.PutAsync($"/departments/{cmdNoName.Id}", JsonContent.Create(cmdNoName));
        await respNoName.AssertValidationProblemAsync(
            errors: new Dictionary<string, string[]> { ["Name"] = ["'Name' must not be empty."] });

        // Route id must match body id
        var deptId = Guid.NewGuid();
        var updateCmd = new UpdateDepartment.Command(deptId, "IT Department");
        var differentId = Guid.NewGuid();
        var updateResp = await HttpClient.PutAsync($"/departments/{differentId}", JsonContent.Create(updateCmd));

        await updateResp.AssertProblemDetailsAsync(
            HttpStatusCode.BadRequest,
            "Route ID mismatch",
            "Route ID does not match request ID",
            $"PUT /departments/{differentId}");
    }

    [Fact]
    public async Task Test_DeleteDepartment_validation()
    {
        // No validation - id comes from route only
    }

    #endregion
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
            .ReadJsonAsync<DataResponse<QueryDepartments.Response[]>>();
        Assert.Empty(departments1.Data);

        // Create department
        var createDeptCmd = new CreateDepartment.Command("New Department");
        var createDeptResp = await HttpClient.PostAsync("/departments", JsonContent.Create(createDeptCmd));
        Assert.Equal(HttpStatusCode.Created, createDeptResp.StatusCode);

        // Extract department ID from response
        var createdDept = await createDeptResp.ReadJsonAsync<EntityCreatedResponse<Guid>>();
        var newDepartmentId = createdDept.Id;

        // Assert department returned in list
        var departments2 = await HttpClient.GetAsync("/departments")
            .ReadJsonAsync<DataResponse<QueryDepartments.Response[]>>();
        Assert.Equal(
            new QueryDepartments.Response[] { new(newDepartmentId, "New Department") },
            departments2.Data);

        // Assert can get department by ID
        var department = await HttpClient.GetAsync($"/departments/{newDepartmentId}")
            .ReadJsonAsync<DataResponse<GetDepartmentById.Response>>();
        Assert.Equal(
            new GetDepartmentById.Response(newDepartmentId, "New Department"),
            department.Data);
    }

    [Fact]
    public async Task Cannot_create_department_with_duplicate_name()
    {
        await LoginAsAdminAsync();

        const string departmentName = "Operations";

        var createCmd = new CreateDepartment.Command(departmentName);
        var createResp = await HttpClient.PostAsync("/departments", JsonContent.Create(createCmd));
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);

        var duplicateResp = await HttpClient.PostAsync("/departments", JsonContent.Create(createCmd));
        await duplicateResp.AssertValidationProblemAsync(
            title: "Department name must be unique.",
            detail: "Department.DuplicateName",
            errors: new Dictionary<string, string[]>
            {
                ["Name"] = [$"Department '{departmentName}' already exists."]
            });

        var departments = await HttpClient.GetAsync("/departments")
            .ReadJsonAsync<DataResponse<QueryDepartments.Response[]>>();
        Assert.Single(departments.Data);
    }

    [Fact]
    public async Task Cannot_update_department_to_existing_name()
    {
        await LoginAsAdminAsync();

        const string duplicateName = "Operations";
        const string marketingName = "Marketing";

        var createOpsCmd = new CreateDepartment.Command(duplicateName);
        var opsResp = await HttpClient.PostAsync("/departments", JsonContent.Create(createOpsCmd));
        Assert.Equal(HttpStatusCode.Created, opsResp.StatusCode);

        var createMarketingCmd = new CreateDepartment.Command(marketingName);
        var marketingResp = await HttpClient.PostAsync("/departments", JsonContent.Create(createMarketingCmd));
        var marketingDept = await marketingResp.ReadJsonAsync<EntityCreatedResponse<Guid>>();
        var marketingId = marketingDept.Id;

        var updateCmd = new UpdateDepartment.Command(marketingId, duplicateName);
        var updateResp = await HttpClient.PutAsync($"/departments/{marketingId}", JsonContent.Create(updateCmd));

        await updateResp.AssertValidationProblemAsync(
            title: "Department name must be unique.",
            detail: "Department.DuplicateName",
            errors: new Dictionary<string, string[]>
            {
                ["Name"] = [$"Department '{duplicateName}' already exists."]
            });

        var marketing = await HttpClient.GetAsync($"/departments/{marketingId}")
            .ReadJsonAsync<DataResponse<GetDepartmentById.Response>>();
        Assert.Equal(marketingName, marketing.Data.Name);
    }

    [Fact]
    public async Task Admin_can_update_department_name()
    {
        await LoginAsAdminAsync();

        // Create department
        var createCmd = new CreateDepartment.Command("Marketing");
        var createResp = await HttpClient.PostAsync("/departments", JsonContent.Create(createCmd));
        var createdDept = await createResp.ReadJsonAsync<EntityCreatedResponse<Guid>>();
        var deptId = createdDept.Id;

        // Update department name
        var updateCmd = new UpdateDepartment.Command(deptId, "Marketing & Sales");
        var updateResp = await HttpClient.PutAsync($"/departments/{deptId}", JsonContent.Create(updateCmd));
        Assert.Equal(HttpStatusCode.NoContent, updateResp.StatusCode);

        // Verify update
        var getDeptResp = await HttpClient.GetAsync($"/departments/{deptId}")
            .ReadJsonAsync<DataResponse<GetDepartmentById.Response>>();
        Assert.Equal("Marketing & Sales", getDeptResp.Data.Name);
    }

    [Fact]
    public async Task Update_department_returns_not_found_for_nonexistent_department()
    {
        await LoginAsAdminAsync();

        var nonexistentId = Guid.NewGuid();
        var updateCmd = new UpdateDepartment.Command(nonexistentId, "Nonexistent Dept");
        var updateResp = await HttpClient.PutAsync($"/departments/{nonexistentId}", JsonContent.Create(updateCmd));

        await updateResp.AssertProblemDetailsAsync(
            HttpStatusCode.NotFound,
            "Department not found",
            "NotFound",
            $"PUT /departments/{nonexistentId}");
    }

    [Fact]
    public async Task Admin_can_delete_empty_department()
    {
        await LoginAsAdminAsync();

        // Create department
        var createCmd = new CreateDepartment.Command("Temporary Dept");
        var createResp = await HttpClient.PostAsync("/departments", JsonContent.Create(createCmd));
        var createdDept = await createResp.ReadJsonAsync<EntityCreatedResponse<Guid>>();
        var deptId = createdDept.Id;

        // Verify department exists in list
        var deptsBefore = await HttpClient.GetAsync("/departments")
            .ReadJsonAsync<DataResponse<QueryDepartments.Response[]>>();
        Assert.Contains(deptsBefore.Data, d => d.Id == deptId);

        // Delete department
        var deleteResp = await HttpClient.DeleteAsync($"/departments/{deptId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResp.StatusCode);

        // Verify deletion - department no longer in list
        var deptsAfter = await HttpClient.GetAsync("/departments")
            .ReadJsonAsync<DataResponse<QueryDepartments.Response[]>>();
        Assert.DoesNotContain(deptsAfter.Data, d => d.Id == deptId);
    }

    [Fact]
    public async Task Cannot_delete_department_with_users()
    {
        await LoginAsAdminAsync();

        // Create department
        var deptCmd = new CreateDepartment.Command("Finance");
        var deptResp = await HttpClient.PostAsync("/departments", JsonContent.Create(deptCmd));
        var createdDept = await deptResp.ReadJsonAsync<EntityCreatedResponse<Guid>>();
        var deptId = createdDept.Id;

        // Create user
        var userCmd = new CreateUser.Command("finance.user@example.com", "Test1234!", "Finance", "User");
        var userResp = await HttpClient.PostAsync("/users", JsonContent.Create(userCmd));
        var createdUser = await userResp.ReadJsonAsync<EntityCreatedResponse<Guid>>();
        var userId = createdUser.Id.ToString();

        // Assign user to department
        var assignCmd = new AssignUserToDepartment.Command(userId, deptId);
        var assignResp = await HttpClient.PatchAsync($"/users/{userId}/department", JsonContent.Create(assignCmd));
        Assert.Equal(HttpStatusCode.NoContent, assignResp.StatusCode);

        // Attempt to delete department - should fail with validation error
        var deleteResp = await HttpClient.DeleteAsync($"/departments/{deptId}");

        await deleteResp.AssertProblemDetailsAsync(
            HttpStatusCode.BadRequest,
            "Cannot delete department. It has 1 user(s). Please reassign users before deleting.",
            "Validation.Error",
            $"DELETE /departments/{deptId}");
    }

    [Fact]
    public async Task Delete_department_returns_not_found_for_nonexistent_department()
    {
        await LoginAsAdminAsync();

        var unknownDeptId = Guid.NewGuid();
        var deleteResp = await HttpClient.DeleteAsync($"/departments/{unknownDeptId}");

        await deleteResp.AssertProblemDetailsAsync(
            HttpStatusCode.NotFound,
            "Department not found",
            "NotFound",
            $"DELETE /departments/{unknownDeptId}");
    }
}
