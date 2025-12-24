using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity.Data;
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
public class UserTestsWithSharedDb(ApiTestHostFixture hostFixture, ITestOutputHelper testOutputHelper, UserSetupFixture userSetupFixture)
    : HttpClientBase(hostFixture, testOutputHelper),
        IClassFixture<ResetDatabaseFixture>,
        IClassFixture<UserSetupFixture>,
        IAsyncLifetime
{
    private const string ValidUserEmail = "user1@example.com";
    private const string ValidUserPassword = "Test1234!";

    private static readonly CreateUser.Request ValidUserCreateRequest = new(ValidUserEmail, ValidUserPassword, "Some", "User");

    public async ValueTask InitializeAsync()
    {
        await userSetupFixture.EnsureUserCreated(this, AdminEmail, AdminPassword, ValidUserEmail, ValidUserPassword);
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public static TheoryData<EndpointInfo> GetAllUserEndpoints()
    {
        // TODO: consider auto-generating this list in future
        return new TheoryData<EndpointInfo>
        {
            new EndpointInfo("/users", "POST", "CreateUser"),
            new EndpointInfo("/users", "GET", "QueryUsers"),
            new EndpointInfo("/users/{id}", "GET", "GetUserById"),
            new EndpointInfo("/users/{id}", "PUT", "UpdateUser"),
            new EndpointInfo("/users/{id}/department", "PATCH", "AssignUserToDepartment"),
            new EndpointInfo("/users/{id}/enable", "PATCH", "EnableUser"),
            new EndpointInfo("/users/{id}/disable", "PATCH", "DisableUser")
        };
    }

    [Theory]
    [MemberData(nameof(GetAllUserEndpoints))]
    public async Task All_user_endpoints_require_authentication(EndpointInfo endpoint)
    {
        // Note: Not logging in as anyone initially

        const string testId = "test-id";

        var path = endpoint.Path.Replace("{id}", testId);
        var request = new HttpRequestMessage(new HttpMethod(endpoint.Method), path);

        var resp = await HttpClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Theory]
    [MemberData(nameof(GetAllUserEndpoints))]
    public async Task All_user_endpoints_require_admin_authorization(EndpointInfo endpoint)
    {
        // Log in as a regular user, not an admin
        await LoginAsync(ValidUserEmail, ValidUserPassword);

        const string testId = "test-id";

        var path = endpoint.Path.Replace("{id}", testId);
        var request = new HttpRequestMessage(new HttpMethod(endpoint.Method), path);

        var resp = await HttpClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    #region Endpoint Validation Tests

    [Theory]
    [MemberData(nameof(GetAllUserEndpoints))]
    public void All_user_endpoints_have_validation_tests(EndpointInfo endpoint)
    {
        // Verify test method exists using reflection
        var testMethod = GetType().GetMethod($"Test_{endpoint.Name}_validation");
        Assert.NotNull(testMethod);
    }

    [Fact]
    public async Task Test_CreateUser_validation()
    {
        // Log in as admin to be able to manage users
        await LoginAsAdminAsync();

        // No email
        var reqNoEmail = ValidUserCreateRequest with { Email = null! };
        var respNoEmail = await HttpClient.PostAsync("/users", JsonContent.Create(reqNoEmail));
        await respNoEmail.AssertValidationProblemAsync(
            errors: new Dictionary<string, string[]> { ["Email"] = ["'Email' must not be empty."] });

        // Not a valid email
        var reqInvalidEmail = ValidUserCreateRequest with { Email = "not-an-email" };
        var respInvalidEmail = await HttpClient.PostAsync("/users", JsonContent.Create(reqInvalidEmail));
        await respInvalidEmail.AssertValidationProblemAsync(
            errors: new Dictionary<string, string[]> { ["Email"] = ["'Email' is not a valid email address."] });

        // No password
        var reqNoPwd = ValidUserCreateRequest with { Password = null! };
        var respNoPwd = await HttpClient.PostAsync("/users", JsonContent.Create(reqNoPwd));
        await respNoPwd.AssertValidationProblemAsync(
            errors: new Dictionary<string, string[]> { ["Password"] = ["'Password' must not be empty."] });
    }

    [Fact]
    public async Task Test_QueryUsers_validation()
    {
        // Log in as admin to be able to manage users
        await LoginAsAdminAsync();

        // Not a valid email
        var respInvalidEmail = await HttpClient.GetAsync("/users?email=not-an-email");
        await respInvalidEmail.AssertValidationProblemAsync(
            errors: new Dictionary<string, string[]> { ["Email"] = ["'Email' is not a valid email address."] });

        // Page < 1
        var respInvalidPage = await HttpClient.GetAsync("/users?page=0");
        await respInvalidPage.AssertValidationProblemAsync(
            errors: new Dictionary<string, string[]> { ["Page"] = ["'Page' must be greater than or equal to '1'."] });

        // Page size < 1
        var respInvalidPageSize1 = await HttpClient.GetAsync("/users?pageSize=0");
        await respInvalidPageSize1.AssertValidationProblemAsync(
            errors: new Dictionary<string, string[]> { ["PageSize"] = ["'Page Size' must be between 1 and 100. Received 0."] });

        // Page size > 100
        var respInvalidPageSize2 = await HttpClient.GetAsync("/users?pageSize=101");
        await respInvalidPageSize2.AssertValidationProblemAsync(
            errors: new Dictionary<string, string[]> { ["PageSize"] = ["'Page Size' must be between 1 and 100. Received 101."] });
    }

    [Fact]
    public async Task Test_GetUserById_validation()
    {
        // Nothing to do
    }

    [Fact]
    public async Task Test_UpdateUser_validation()
    {
        await LoginAsAdminAsync();

        // Route id must match body id for update user
        const string user1Id = "user-id-1";
        const string user2Id = "user-id-2";

        var updateReq = new UpdateUser.Request(user1Id, "test@example.com", "Test", "User");
        var resp = await HttpClient.PutAsync($"/users/{user2Id}", JsonContent.Create(updateReq));

        await resp.AssertProblemDetailsAsync(
            HttpStatusCode.BadRequest,
            "Route ID mismatch",
            "Route ID does not match request ID",
            $"PUT /users/{user2Id}");
    }


    [Fact]
    public async Task Test_AssignUserToDepartment_validation()
    {
        await LoginAsAdminAsync();

        // Route id must match body id for assign department
        const string user1Id = "user-id-1";
        const string user2Id = "user-id-2";
        ;

        var assignReq = new AssignUserToDepartment.Request(user1Id, 123);
        var resp = await HttpClient.PatchAsync($"/users/{user2Id}/department", JsonContent.Create(assignReq));

        await resp.AssertProblemDetailsAsync(
            HttpStatusCode.BadRequest,
            "Route ID mismatch",
            "Route ID does not match request ID",
            $"PATCH /users/{user2Id}/department");
    }

    [Fact]
    public async Task Test_EnableUser_validation()
    {
        // Id comes from route only - nothing to validate
    }

    [Fact]
    public async Task Test_DisableUser_validation()
    {
        // Id comes from route only - nothing to validate
    }

    #endregion
}

[Collection("ApiTestHost")]
public class UserTests(ApiTestHostFixture hostFixture, ITestOutputHelper testOutputHelper)
    : HttpClientAndDbResetBase(hostFixture, testOutputHelper)
{
    private const string ValidUserEmail = "user1@example.com";
    private const string ValidUserPassword = "Test1234!";

    private static readonly CreateUser.Request ValidCreateRequest = new(ValidUserEmail, ValidUserPassword, "Some", "User");

    /// <summary>
    /// Note: In a real-world system, this would use invite emails and a time-limited invite token.
    /// Keeping it simple here for demo.
    /// </summary>
    [Fact]
    public async Task Admin_user_can_create_user()
    {
        // Log in as admin to be able to manage users
        await LoginAsAdminAsync();

        var newUserEmailMixedCase = "User1@Example.COM";
        var newUserEmailMixedCase2 = "USER1@example.com";
        var newUserEmailLower = newUserEmailMixedCase.ToLowerInvariant();

        // Search for new user by email -> No result
        var userList1 = await HttpClient.GetAsync($"/users?email={newUserEmailMixedCase}")
            .ReadJsonAsync<PagedResponse<QueryUsers.Response>>();
        Assert.Empty(userList1.Data);

        // Admin creates user
        var newUserReq = ValidCreateRequest with { Email = newUserEmailMixedCase };
        var regResp = await HttpClient.PostAsync("/users", JsonContent.Create(newUserReq));
        Assert.Equal(HttpStatusCode.Created, regResp.StatusCode);

        // Ensure Location header contains new user ID
        var location = regResp.Headers.Location?.ToString();
        Assert.Matches(@"^/users/[0-9a-f-]+$", location);
        var newUserId = location!.Split('/').Last();

        // Search for new user by email -> Found
        var userList2 = await HttpClient.GetAsync($"/users?email={newUserEmailMixedCase2}")
            .ReadJsonAsync<PagedResponse<QueryUsers.Response>>();
        var newUser = Assert.Single(userList2.Data);
        Assert.Equal(newUserId, newUser.Id);
        Assert.Equal(newUserEmailLower, newUser.Email);

        // Can get new user by ID
        var userById = await HttpClient.GetAsync($"/users/{newUserId}")
            .ReadJsonAsync<DataResponse<GetUserById.Response>>();
        Assert.Equal(newUserId, userById.Data.Id);
        Assert.Equal(newUserEmailLower, userById!.Data.Email);
    }

    [Fact]
    public async Task Cannot_create_user_with_duplicate_email()
    {
        const string email = ValidUserEmail;
        const string password = ValidUserPassword;

        // Log in as admin to be able to manage users
        await LoginAsAdminAsync();

        var newUserReq = ValidCreateRequest with { Email = email, Password = password };

        // Attempt 1: Admin creates user - should work
        var regResp1 = await HttpClient.PostAsync("/users", JsonContent.Create(newUserReq));
        Assert.Equal(HttpStatusCode.Created, regResp1.StatusCode);

        // Attempt 2: Admin tries to create user with same email - should fail
        var regResp2 = await HttpClient.PostAsync("/users", JsonContent.Create(newUserReq));
        await regResp2.AssertValidationProblemAsync(
            errors: new Dictionary<string, string[]>
            {
                ["DuplicateUserName"] = [$"Username '{email}' is already taken."]
            });

        // Assert only 1 user record created
        var userList2 = await HttpClient.GetAsync($"/users?email={email}")
            .ReadJsonAsync<PagedResponse<QueryUsers.Response>>();
        var newUser2 = Assert.Single(userList2.Data);
        Assert.Equal(email, newUser2.Email);
    }

    [Fact]
    public async Task User_can_login()
    {
        // Log in as admin to be able to manage users
        await LoginAsAdminAsync();

        // Confirm logged in as admin: 
        var info1 = await HttpClient.GetAsync("/manage/info")
            .ReadJsonAsync<InfoResponse>();
        Assert.Equal(AdminEmail, info1.Email);

        // Admin creates user
        var regResp1 = await HttpClient.PostAsync("/users", JsonContent.Create(ValidCreateRequest));
        Assert.Equal(HttpStatusCode.Created, regResp1.StatusCode);

        // Log in as user
        await LoginAsync(ValidCreateRequest.Email, ValidCreateRequest.Password);

        // Confirm logged in as user: 
        var info2 = await HttpClient.GetAsync("/manage/info")
            .ReadJsonAsync<InfoResponse>();
        Assert.Equal(ValidCreateRequest.Email, info2.Email);
    }

    [Fact]
    public async Task Admin_can_update_user_profile()
    {
        await LoginAsAdminAsync();

        // Create user
        var createResp = await HttpClient.PostAsync("/users", JsonContent.Create(ValidCreateRequest));
        var userId = createResp.Headers.Location!.ToString().Split('/').Last();

        // Update user profile
        var updateReq = new UpdateUser.Request(userId, "updated@example.com", "Updated", "Name");
        var updateResp = await HttpClient.PutAsync($"/users/{userId}", JsonContent.Create(updateReq));
        Assert.Equal(HttpStatusCode.NoContent, updateResp.StatusCode);

        // Verify update
        var user = await HttpClient.GetAsync($"/users/{userId}")
            .ReadJsonAsync<DataResponse<GetUserById.Response>>();
        Assert.Equal("updated@example.com", user.Data.Email);
        Assert.Equal("Updated", user.Data.FirstName);
        Assert.Equal("Name", user.Data.LastName);
    }

    [Fact]
    public async Task Admin_can_enable_and_disable_user()
    {
        await LoginAsAdminAsync();

        // Create user (enabled by default)
        var createResp = await HttpClient.PostAsync("/users", JsonContent.Create(ValidCreateRequest));
        var userId = createResp.Headers.Location!.ToString().Split('/').Last();

        // User can login initially
        await LoginAsync(ValidCreateRequest.Email, ValidCreateRequest.Password);

        // Switch back to admin
        await LoginAsAdminAsync();

        // Disable user
        var disableResp = await HttpClient.PatchAsync($"/users/{userId}/disable", null);
        Assert.Equal(HttpStatusCode.NoContent, disableResp.StatusCode);

        // Disabled user cannot log in
        var loginReq = new LoginRequest { Email = ValidCreateRequest.Email, Password = ValidCreateRequest.Password };
        var loginResp = await HttpClient.PostAsync("/login?useCookies=true", JsonContent.Create(loginReq));
        Assert.Equal(HttpStatusCode.Unauthorized, loginResp.StatusCode);

        // Switch back to admin
        await LoginAsAdminAsync();

        // Enable user
        var enableResp = await HttpClient.PatchAsync($"/users/{userId}/enable", null);
        Assert.Equal(HttpStatusCode.NoContent, enableResp.StatusCode);

        // User can login again
        await LoginAsync(ValidCreateRequest.Email, ValidCreateRequest.Password);

        // Switch back to admin and enable again (idempotent)
        await LoginAsAdminAsync();
        var enableResp2 = await HttpClient.PatchAsync($"/users/{userId}/enable", null);
        Assert.Equal(HttpStatusCode.NoContent, enableResp2.StatusCode);
    }

    [Fact]
    public async Task Admin_can_assign_user_to_department()
    {
        await LoginAsAdminAsync();

        // Create department
        var createDeptReq = new CreateDepartment.Request("Engineering");
        var createDeptResp = await HttpClient.PostAsync("/departments", JsonContent.Create(createDeptReq));
        var deptId = int.Parse(createDeptResp.Headers.Location!.ToString().Split('/').Last());

        // Create user
        var createUserResp = await HttpClient.PostAsync("/users", JsonContent.Create(ValidCreateRequest));
        var userId = createUserResp.Headers.Location!.ToString().Split('/').Last();

        // Assert no department assigned initially
        var initialUser = await HttpClient.GetAsync($"/users/{userId}")
            .ReadJsonAsync<DataResponse<GetUserById.Response>>();
        Assert.Null(initialUser.Data.Department);

        // Assign user to department
        var assignReq = new AssignUserToDepartment.Request(userId, deptId);
        var assignResp = await HttpClient.PatchAsync($"/users/{userId}/department", JsonContent.Create(assignReq));
        Assert.Equal(HttpStatusCode.NoContent, assignResp.StatusCode);

        // Verify assignment
        var user = await HttpClient.GetAsync($"/users/{userId}")
            .ReadJsonAsync<DataResponse<GetUserById.Response>>();
        Assert.NotNull(user.Data.Department);
        Assert.Equal(deptId, user.Data.Department.Id);
        Assert.Equal("Engineering", user.Data.Department.Name);

        // Unassign user from department (set to null)
        var unassignReq = new AssignUserToDepartment.Request(userId, null);
        var unassignResp = await HttpClient.PatchAsync($"/users/{userId}/department", JsonContent.Create(unassignReq));
        Assert.Equal(HttpStatusCode.NoContent, unassignResp.StatusCode);

        // Verify unassignment
        var user2 = await HttpClient.GetAsync($"/users/{userId}")
            .ReadJsonAsync<DataResponse<GetUserById.Response>>();
        Assert.Null(user2.Data.Department);
    }

    [Fact]
    public async Task Cannot_assign_user_to_nonexistent_department()
    {
        await LoginAsAdminAsync();

        // Create user
        var createUserResp = await HttpClient.PostAsync("/users", JsonContent.Create(ValidCreateRequest));
        var userId = createUserResp.Headers.Location!.ToString().Split('/').Last();

        // Try to assign user to non-existent department
        const int departmentId = 99999;
        var assignReq = new AssignUserToDepartment.Request(userId, departmentId);
        var assignResp = await HttpClient.PatchAsync($"/users/{userId}/department", JsonContent.Create(assignReq));
        await assignResp.AssertProblemDetailsAsync(
            HttpStatusCode.NotFound,
            detail: "Department.NotFound",
            title: $"Department with ID '{departmentId}' not found",
            instance: $"PATCH /users/{userId}/department");
    }

    [Fact]
    public async Task Update_user_returns_404_for_nonexistent_user()
    {
        await LoginAsAdminAsync();

        const string userId = "nonexistent-id";
        var updateReq = new UpdateUser.Request("nonexistent-id", "test@example.com", "Test", "User");
        var updateResp = await HttpClient.PutAsync($"/users/{userId}", JsonContent.Create(updateReq));
        await updateResp.AssertProblemDetailsAsync(
            HttpStatusCode.NotFound,
            detail: "User.NotFound",
            title: $"User with ID '{userId}' not found",
            instance: $"PUT /users/{userId}");
    }

    [Fact]
    public async Task Enable_user_returns_404_for_nonexistent_user()
    {
        await LoginAsAdminAsync();

        var enableResp = await HttpClient.PatchAsync("/users/nonexistent-id/enable", null);
        await enableResp.AssertProblemDetailsAsync(
            HttpStatusCode.NotFound,
            detail: "User.NotFound");
    }

    [Fact]
    public async Task Disable_user_returns_404_for_nonexistent_user()
    {
        await LoginAsAdminAsync();

        var disableResp = await HttpClient.PatchAsync("/users/nonexistent-id/disable", null);
        await disableResp.AssertProblemDetailsAsync(
            HttpStatusCode.NotFound,
            detail: "User.NotFound");
    }

    [Fact]
    public async Task Get_user_includes_roles()
    {
        await LoginAsAdminAsync();

        // Get admin user (should have Admin role)
        var adminUsers = await HttpClient.GetAsync($"/users?email={AdminEmail}")
            .ReadJsonAsync<PagedResponse<QueryUsers.Response>>();
        var adminUser = Assert.Single(adminUsers.Data);
        Assert.Contains("Admin", adminUser.Roles);

        // Create regular user (should have no roles)
        var createUserResp = await HttpClient.PostAsync("/users", JsonContent.Create(ValidCreateRequest));
        var userId = createUserResp.Headers.Location!.ToString().Split('/').Last();

        var user = await HttpClient.GetAsync($"/users/{userId}")
            .ReadJsonAsync<DataResponse<GetUserById.Response>>();
        Assert.Empty(user.Data.Roles);
    }
}
