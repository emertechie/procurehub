using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity.Data;
using ProcureHub.Features.Users;
using ProcureHub.WebApi.ApiResponses;
using ProcureHub.WebApi.Tests.Infrastructure;

namespace ProcureHub.WebApi.Tests.Features;

/// <summary>
/// NOTE: DB is only reset once per class, so only use for tests that don't persist state
/// </summary>
[Collection("ApiTestHost")]
public class UserTestsWithSharedDb(ApiTestHostFixture hostFixture, ITestOutputHelper testOutputHelper)
    : HttpClientBase(hostFixture, testOutputHelper), IClassFixture<ResetDatabaseFixture>
{
    private const string ValidUserEmail = "user1@example.com";
    private const string ValidUserPassword = "Test1234!";

    private static readonly CreateUser.Request ValidCreateRequest = new(ValidUserEmail, ValidUserPassword, "Some", "User");

    [Fact]
    public async Task Test_validation_for_all_endpoints()
    {
        // Log in as admin to be able to manage users
        await LoginAsAdminAsync();

        await TestHelper.RunTestsForAllAsync<AllUserEndpoints>(configure =>
        {
            configure.CreateUser = TestCreateUserEndpoint;
            configure.QueryUsers = TestQueryUsersEndpoint;
            configure.GetUserById = TestGetUserByIdEndpoint;
        });

        async Task TestCreateUserEndpoint()
        {
            // No email
            var reqNoEmail = ValidCreateRequest with { Email = null! };
            var respNoEmail = await HttpClient.PostAsync("/users", JsonContent.Create(reqNoEmail), CancellationToken);
            await respNoEmail.AssertValidationProblemAsync(CancellationToken,
                errors: new Dictionary<string, string[]> { ["Email"] = ["'Email' must not be empty."] });

            // Not a valid email
            var reqInvalidEmail = ValidCreateRequest with { Email = "not-an-email" };
            var respInvalidEmail = await HttpClient.PostAsync("/users", JsonContent.Create(reqInvalidEmail), CancellationToken);
            await respInvalidEmail.AssertValidationProblemAsync(CancellationToken,
                errors: new Dictionary<string, string[]> { ["Email"] = ["'Email' is not a valid email address."] });

            // No password
            var reqNoPwd = ValidCreateRequest with { Password = null! };
            var respNoPwd = await HttpClient.PostAsync("/users", JsonContent.Create(reqNoPwd), CancellationToken);
            await respNoPwd.AssertValidationProblemAsync(CancellationToken,
                errors: new Dictionary<string, string[]> { ["Password"] = ["'Password' must not be empty."] });
        }

        async Task TestQueryUsersEndpoint()
        {
            // Not a valid email
            var respInvalidEmail = await HttpClient.GetAsync("/users?email=not-an-email", CancellationToken);
            await respInvalidEmail.AssertValidationProblemAsync(CancellationToken,
                errors: new Dictionary<string, string[]> { ["Email"] = ["'Email' is not a valid email address."] });

            // Page < 1
            var respInvalidPage = await HttpClient.GetAsync("/users?page=0", CancellationToken);
            await respInvalidPage.AssertValidationProblemAsync(CancellationToken,
                errors: new Dictionary<string, string[]> { ["Page"] = ["'Page' must be greater than or equal to '1'."] });

            // Page size < 1
            var respInvalidPageSize1 = await HttpClient.GetAsync("/users?pageSize=0", CancellationToken);
            await respInvalidPageSize1.AssertValidationProblemAsync(CancellationToken,
                errors: new Dictionary<string, string[]> { ["PageSize"] = ["'Page Size' must be between 1 and 100. Received 0."] });

            // Page size > 100
            var respInvalidPageSize2 = await HttpClient.GetAsync("/users?pageSize=101", CancellationToken);
            await respInvalidPageSize2.AssertValidationProblemAsync(CancellationToken,
                errors: new Dictionary<string, string[]> { ["PageSize"] = ["'Page Size' must be between 1 and 100. Received 101."] });
        }

        Task TestGetUserByIdEndpoint()
        {
            // ID not provided -> not possible to test this as it just maps to the "/users" endpoint 
            // var respInvalidEmail = await HttpClient.GetAsync("/users/", CancellationToken);

            // ReSharper disable once ConvertToLambdaExpression
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Ensure_authentication_required_for_all_endpoints()
    {
        // Note: Not logging in as admin - to test unauth access.
        // await LoginAsAdminAsync();

        // Ensure you need to be authenticated to access all /users endpoints
        await TestHelper.RunTestsForAllAsync<AllUserEndpoints>(configure =>
        {
            configure.CreateUser = async () =>
            {
                var req = JsonContent.Create(ValidCreateRequest);
                var resp = await HttpClient.PostAsync("/users", req, CancellationToken);
                Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
            };
            configure.QueryUsers = async () =>
            {
                var resp = await HttpClient.GetAsync("/users", CancellationToken);
                Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
            };
            configure.GetUserById = async () =>
            {
                var resp = await HttpClient.GetAsync("/users/1", CancellationToken);
                Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
            };
        });
    }
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
        var listUsersResp1 = await HttpClient.GetAsync($"/users?email={newUserEmailMixedCase}", CancellationToken);
        var userList1 = await listUsersResp1.AssertSuccessAndReadJsonAsync<ApiPagedResponse<QueryUsers.Response>>(CancellationToken);
        Assert.Empty(userList1!.Data);

        // Admin creates user
        var newUserReq = ValidCreateRequest with { Email = newUserEmailMixedCase };
        var regResp = await HttpClient.PostAsync("/users", JsonContent.Create(newUserReq), CancellationToken);
        Assert.Equal(HttpStatusCode.Created, regResp.StatusCode);

        // Ensure Location header contains new user ID
        var location = regResp.Headers.Location?.ToString();
        Assert.Matches(@"^/users/[0-9a-f-]+$", location);
        var newUserId = location!.Split('/').Last();

        // Search for new user by email -> Found
        var listUsersResp2 = await HttpClient.GetAsync($"/users?email={newUserEmailMixedCase2}", CancellationToken);
        var userList2 = await listUsersResp2.AssertSuccessAndReadJsonAsync<ApiPagedResponse<QueryUsers.Response>>(CancellationToken);
        var newUser = Assert.Single(userList2!.Data);
        Assert.Equal(newUserId, newUser.Id);
        Assert.Equal(newUserEmailLower, newUser.Email);

        // Can get new user by ID
        var getUserResp = await HttpClient.GetAsync($"/users/{newUserId}", CancellationToken);
        var userById = await getUserResp.AssertSuccessAndReadJsonAsync<ApiDataResponse<GetUserById.Response>>(CancellationToken);
        Assert.Equal(newUserId, userById!.Data.Id);
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
        var regResp1 = await HttpClient.PostAsync("/users", JsonContent.Create(newUserReq), CancellationToken);
        Assert.Equal(HttpStatusCode.Created, regResp1.StatusCode);

        // Attempt 2: Admin tries to create user with same email - should fail
        var regResp2 = await HttpClient.PostAsync("/users", JsonContent.Create(newUserReq), CancellationToken);
        await regResp2.AssertValidationProblemAsync(CancellationToken,
            errors: new Dictionary<string, string[]>
            {
                ["DuplicateUserName"] = [$"Username '{email}' is already taken."]
            });

        // Assert only 1 user record created
        var listUsersResp2 = await HttpClient.GetAsync($"/users?email={email}", CancellationToken);
        var userList2 = await listUsersResp2.AssertSuccessAndReadJsonAsync<ApiPagedResponse<QueryUsers.Response>>(CancellationToken);
        var newUser2 = Assert.Single(userList2!.Data);
        Assert.Equal(email, newUser2.Email);
    }

    [Fact]
    public async Task User_can_login()
    {
        // Log in as admin to be able to manage users
        await LoginAsAdminAsync();

        // Confirm logged in as admin: 
        var infoResp1 = await HttpClient.GetAsync("/manage/info", CancellationToken);
        var info1 = await infoResp1.AssertSuccessAndReadJsonAsync<InfoResponse>(CancellationToken);
        Assert.Equal(AdminEmail, info1!.Email);

        // Admin creates user
        var regResp1 = await HttpClient.PostAsync("/users", JsonContent.Create(ValidCreateRequest), CancellationToken);
        Assert.Equal(HttpStatusCode.Created, regResp1.StatusCode);

        // Log in as user
        await LoginAsync(ValidCreateRequest.Email, ValidCreateRequest.Password);

        // Confirm logged in as user: 
        var infoResp2 = await HttpClient.GetAsync("/manage/info", CancellationToken);
        var info2 = await infoResp2.AssertSuccessAndReadJsonAsync<InfoResponse>(CancellationToken);
        Assert.Equal(ValidCreateRequest.Email, info2!.Email);
    }

    [Fact]
    public async Task Users_cannot_access_user_endpoints()
    {
        const string email1 = "user1@example.com";
        const string email2 = "user2@example.com";

        // Log in as admin to be able to manage users
        await LoginAsAdminAsync();

        // Admin creates user 1
        var newUser1ReqAdmin = ValidCreateRequest with { Email = email1 };
        var createResp1Admin = await HttpClient.PostAsync("/users", JsonContent.Create(newUser1ReqAdmin), CancellationToken);
        Assert.Equal(HttpStatusCode.Created, createResp1Admin.StatusCode);
        var user1Id = createResp1Admin.Headers.Location!.ToString().Split('/').Last();

        // Admin creates user 2
        var newUser2ReqAdmin = ValidCreateRequest with { Email = email2 };
        var createResp2Admin = await HttpClient.PostAsync("/users", JsonContent.Create(newUser2ReqAdmin), CancellationToken);
        Assert.Equal(HttpStatusCode.Created, createResp2Admin.StatusCode);
        var user2Id = createResp2Admin.Headers.Location!.ToString().Split('/').Last();

        // Log in as user 1
        await LoginAsync(email1, ValidUserPassword);

        await TestHelper.RunTestsForAllAsync<AllUserEndpoints>(configure =>
        {
            configure.CreateUser = async () =>
            {
                // User 1 tries to create another user - should fail
                var newUserReq = ValidCreateRequest;
                var createResp = await HttpClient.PostAsync("/users", JsonContent.Create(newUserReq), CancellationToken);
                await createResp.AssertProblemDetailsAsync(
                    HttpStatusCode.Forbidden,
                    CancellationToken,
                    "Forbidden",
                    instance: "POST /users");
            };
            configure.QueryUsers = async () =>
            {
                // Try to query all users - should fail
                var queryUsersResp = await HttpClient.GetAsync("/users", CancellationToken);
                await queryUsersResp.AssertProblemDetailsAsync(
                    HttpStatusCode.Forbidden,
                    CancellationToken,
                    "Forbidden",
                    instance: "GET /users");
            };
            configure.GetUserById = async () =>
            {
                // Try to get own user record by ID - should fail
                var getUser1Resp = await HttpClient.GetAsync($"/users/{user1Id}", CancellationToken);
                await getUser1Resp.AssertProblemDetailsAsync(
                    HttpStatusCode.Forbidden,
                    CancellationToken,
                    "Forbidden",
                    instance: $"GET /users/{user1Id}");

                // Try to get user 2 record by ID - should fail
                var getUser2Resp = await HttpClient.GetAsync($"/users/{user2Id}", CancellationToken);
                await getUser2Resp.AssertProblemDetailsAsync(
                    HttpStatusCode.Forbidden,
                    CancellationToken,
                    "Forbidden",
                    instance: $"GET /users/{user2Id}");
            };
        });
    }
}

internal class AllUserEndpoints
{
    public Func<Task> CreateUser { get; set; } = null!;
    public Func<Task> QueryUsers { get; set; } = null!;
    public Func<Task> GetUserById { get; set; } = null!;
}
