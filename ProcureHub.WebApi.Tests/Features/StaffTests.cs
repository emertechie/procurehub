using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity.Data;
using ProcureHub.Features.Staff;
using ProcureHub.WebApi.ApiResponses;
using ProcureHub.WebApi.Tests.Infrastructure;

namespace ProcureHub.WebApi.Tests.Features;

[Collection("Integration Tests")]
public class StaffTests(ITestOutputHelper testOutputHelper, IntegrationTestFixture fixture)
    : IntegrationTestsBase(testOutputHelper, fixture)
{
    private const string ValidStaffEmail = "staff1@example.com";
    private const string ValidStaffPassword = "Test1234!";

    [Fact]
    public async Task Test_validation_for_all_endpoints()
    {
        // Log in as admin to be able to manage staff
        await LoginAsAdminAsync();

        await TestHelper.RunTestsForAllAsync<AllStaffEndpoints>(configure =>
        {
            configure.CreateStaff = TestCreateStaffEndpoint;
            configure.QueryStaff = TestQueryStaffEndpoint;
            configure.GetStaffById = TestGetStaffByIdEndpoint;
        });

        async Task TestCreateStaffEndpoint()
        {
            // No email
            var reqNoEmail = new CreateStaff.Request(null, ValidStaffPassword);
            var respNoEmail = await HttpClient.PostAsync("/staff", JsonContent.Create(reqNoEmail), CancellationToken);
            await respNoEmail.AssertValidationProblemAsync(CancellationToken,
                errors: new Dictionary<string, string[]> { ["Email"] = ["'Email' must not be empty."] });

            // Not a valid email
            var reqInvalidEmail = new CreateStaff.Request("not-an-email", ValidStaffPassword);
            var respInvalidEmail =
                await HttpClient.PostAsync("/staff", JsonContent.Create(reqInvalidEmail), CancellationToken);
            await respInvalidEmail.AssertValidationProblemAsync(CancellationToken,
                errors: new Dictionary<string, string[]> { ["Email"] = ["'Email' is not a valid email address."] });

            // No password
            var reqNoPwd = new CreateStaff.Request(ValidStaffEmail, null);
            var respNoPwd = await HttpClient.PostAsync("/staff", JsonContent.Create(reqNoPwd), CancellationToken);
            await respNoPwd.AssertValidationProblemAsync(CancellationToken,
                errors: new Dictionary<string, string[]> { ["Password"] = ["'Password' must not be empty."] });
        }

        async Task TestQueryStaffEndpoint()
        {
            // Not a valid email
            var respInvalidEmail = await HttpClient.GetAsync("/staff?email=not-an-email", CancellationToken);
            await respInvalidEmail.AssertValidationProblemAsync(CancellationToken,
                errors: new Dictionary<string, string[]> { ["Email"] = ["'Email' is not a valid email address."] });

            // Page < 1
            var respInvalidPage = await HttpClient.GetAsync("/staff?page=0", CancellationToken);
            await respInvalidPage.AssertValidationProblemAsync(CancellationToken,
                errors: new Dictionary<string, string[]> { ["Page"] = ["'Page' must be greater than or equal to '1'."] });

            // Page size < 1
            var respInvalidPageSize1 = await HttpClient.GetAsync("/staff?pageSize=0", CancellationToken);
            await respInvalidPageSize1.AssertValidationProblemAsync(CancellationToken,
                errors: new Dictionary<string, string[]> { ["PageSize"] = ["'Page Size' must be between 1 and 100. Received 0."] });

            // Page size > 100
            var respInvalidPageSize2 = await HttpClient.GetAsync("/staff?pageSize=101", CancellationToken);
            await respInvalidPageSize2.AssertValidationProblemAsync(CancellationToken,
                errors: new Dictionary<string, string[]> { ["PageSize"] = ["'Page Size' must be between 1 and 100. Received 101."] });
        }

        Task TestGetStaffByIdEndpoint()
        {
            // ID not provided -> not possible to test this as it just maps to the "/staff" endpoint 
            // var respInvalidEmail = await HttpClient.GetAsync("/staff/", CancellationToken);

            // ReSharper disable once ConvertToLambdaExpression
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Ensure_authentication_required_for_all_endpoints()
    {
        // Note: Not logging in as admin - to test unauth access.
        // await LoginAsAdminAsync();

        // Ensure you need to be authenticated to access all /staff endpoints
        await TestHelper.RunTestsForAllAsync<AllStaffEndpoints>(configure =>
        {
            configure.CreateStaff = async () =>
            {
                var req = JsonContent.Create(new { email = ValidStaffEmail, password = ValidStaffPassword });
                var resp = await HttpClient.PostAsync("/staff", req, CancellationToken);
                Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
            };
            configure.QueryStaff = async () =>
            {
                var resp = await HttpClient.GetAsync("/staff", CancellationToken);
                Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
            };
            configure.GetStaffById = async () =>
            {
                var resp = await HttpClient.GetAsync("/staff/1", CancellationToken);
                Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
            };
        });
    }

    /// <summary>
    /// Note: In a real-world system, this would use invite emails and a time-limited invite token.
    /// Keeping it simple here for demo.
    /// </summary>
    [Fact]
    public async Task Admin_user_can_create_staff_user()
    {
        // Log in as admin to be able to manage staff
        await LoginAsAdminAsync();

        var newUserEmailMixedCase = "Staff1@Example.COM";
        var newUserEmailLower = newUserEmailMixedCase.ToLowerInvariant();

        // Search for new Staff by email -> No result
        var listStaffResp1 = await HttpClient.GetAsync($"/staff?email={newUserEmailMixedCase}", CancellationToken);
        var staffList1 = await listStaffResp1.AssertSuccessAndReadJsonAsync<ApiPagedResponse<QueryStaff.Response>>(CancellationToken);
        Assert.Empty(staffList1!.Data);

        // Admin creates Staff user
        var newStaffReq = JsonContent.Create(new { email = newUserEmailMixedCase, password = "Test1234!" });
        var regResp = await HttpClient.PostAsync("/staff", newStaffReq, CancellationToken);
        Assert.Equal(HttpStatusCode.Created, regResp.StatusCode);

        // Ensure Location header contains new user ID
        var location = regResp.Headers.Location?.ToString();
        Assert.Matches(@"^/staff/[0-9a-f-]+$", location);
        var newUserId = location!.Split('/').Last();

        // Search for new Staff by email -> Found
        var listStaffResp2 = await HttpClient.GetAsync($"/staff?email={newUserEmailMixedCase}", CancellationToken);
        var staffList2 = await listStaffResp2.AssertSuccessAndReadJsonAsync<ApiPagedResponse<QueryStaff.Response>>(CancellationToken);
        var newStaff = Assert.Single(staffList2!.Data);
        Assert.Equal(newUserId, newStaff.Id);
        Assert.Equal(newUserEmailLower, newStaff.Email);

        // Can get new Staff by ID
        var getStaffResp = await HttpClient.GetAsync($"/staff/{newUserId}", CancellationToken);
        var staffById = await getStaffResp.AssertSuccessAndReadJsonAsync<ApiDataResponse<GetStaffById.Response>>(CancellationToken);
        Assert.Equal(newUserId, staffById!.Data.Id);
        Assert.Equal(newUserEmailLower, staffById!.Data.Email);
    }

    [Fact]
    public async Task Cannot_create_staff_member_with_duplicate_email()
    {
        const string email = ValidStaffEmail;
        const string password = ValidStaffPassword;

        // Log in as admin to be able to manage staff
        await LoginAsAdminAsync();

        // Attempt 1: Admin creates user - should work
        var newStaffReq1 = JsonContent.Create(new { email, password });
        var regResp1 = await HttpClient.PostAsync("/staff", newStaffReq1, CancellationToken);
        Assert.Equal(HttpStatusCode.Created, regResp1.StatusCode);

        // Attempt 2: Admin creates user - should *fail*
        var newStaffReq2 = JsonContent.Create(new { email, password });
        var regResp2 = await HttpClient.PostAsync("/staff", newStaffReq2, CancellationToken);
        await regResp2.AssertValidationProblemAsync(CancellationToken,
            errors: new Dictionary<string, string[]>
            {
                ["DuplicateUserName"] = [$"Username '{email}' is already taken."]
            });

        // Assert only 1 Staff record created
        var listStaffResp2 = await HttpClient.GetAsync($"/staff?email={email}", CancellationToken);
        var staffList2 = await listStaffResp2.AssertSuccessAndReadJsonAsync<ApiPagedResponse<QueryStaff.Response>>(CancellationToken);
        var newStaff2 = Assert.Single(staffList2!.Data);
        Assert.Equal(email, newStaff2.Email);
    }

    [Fact]
    public async Task Staff_member_can_login()
    {
        const string email = ValidStaffEmail;
        const string password = ValidStaffPassword;

        // Log in as admin to be able to manage staff
        await LoginAsAdminAsync();

        // Confirm logged in as admin: 
        var infoResp1 = await HttpClient.GetAsync("/manage/info", CancellationToken);
        var info1 = await infoResp1.AssertSuccessAndReadJsonAsync<InfoResponse>(CancellationToken);
        Assert.Equal(AdminEmail, info1!.Email);

        // Admin creates user
        var newStaffReq1 = JsonContent.Create(new { email, password });
        var regResp1 = await HttpClient.PostAsync("/staff", newStaffReq1, CancellationToken);
        Assert.Equal(HttpStatusCode.Created, regResp1.StatusCode);

        // Log in as Staff member
        await LoginAsync(email, password);

        // Confirm logged in as staff: 
        var infoResp2 = await HttpClient.GetAsync("/manage/info", CancellationToken);
        var info2 = await infoResp2.AssertSuccessAndReadJsonAsync<InfoResponse>(CancellationToken);
        Assert.Equal(email, info2!.Email);
    }

    [Fact]
    public async Task Staff_members_cannot_access_staff_endpoints()
    {
        const string email1 = "staff1@example.com";
        const string email2 = "staff2@example.com";

        // Log in as admin to be able to manage staff
        await LoginAsAdminAsync();

        // Admin creates Staff 1
        var newStaff1ReqAdmin = JsonContent.Create(new { email = email1, password = ValidStaffPassword });
        var createResp1Admin = await HttpClient.PostAsync("/staff", newStaff1ReqAdmin, CancellationToken);
        Assert.Equal(HttpStatusCode.Created, createResp1Admin.StatusCode);
        var user1Id = createResp1Admin.Headers.Location!.ToString().Split('/').Last();

        // Admin creates Staff 1
        var newStaff2ReqAdmin = JsonContent.Create(new { email = email2, password = ValidStaffPassword });
        var createResp2Admin = await HttpClient.PostAsync("/staff", newStaff2ReqAdmin, CancellationToken);
        Assert.Equal(HttpStatusCode.Created, createResp2Admin.StatusCode);
        var user2Id = createResp2Admin.Headers.Location!.ToString().Split('/').Last();

        // Log in as Staff 1
        await LoginAsync(email1, ValidStaffPassword);

        await TestHelper.RunTestsForAllAsync<AllStaffEndpoints>(configure =>
        {
            configure.CreateStaff = async () =>
            {
                // Staff 1 tries to create another staff member - should fail
                var newStaffReq = JsonContent.Create(new { email = ValidStaffEmail, password = ValidStaffPassword });
                var createResp = await HttpClient.PostAsync("/staff", newStaffReq, CancellationToken);
                await createResp.AssertProblemDetailsAsync(
                    HttpStatusCode.Forbidden,
                    CancellationToken,
                    "Forbidden",
                    instance: "POST /staff");
            };
            configure.QueryStaff = async () =>
            {
                // Try to query all staff - should fail
                var queryStaffResp = await HttpClient.GetAsync("/staff", CancellationToken);
                await queryStaffResp.AssertProblemDetailsAsync(
                    HttpStatusCode.Forbidden,
                    CancellationToken,
                    "Forbidden",
                    instance: "GET /staff");
            };
            configure.GetStaffById = async () =>
            {
                // Try to get own staff record by ID - should fail
                var getStaff1Resp = await HttpClient.GetAsync($"/staff/{user1Id}", CancellationToken);
                await getStaff1Resp.AssertProblemDetailsAsync(
                    HttpStatusCode.Forbidden,
                    CancellationToken,
                    "Forbidden",
                    instance: $"GET /staff/{user1Id}");

                // Try to get staff 2 record by ID - should fail
                var getStaff2Resp = await HttpClient.GetAsync($"/staff/{user2Id}", CancellationToken);
                await getStaff2Resp.AssertProblemDetailsAsync(
                    HttpStatusCode.Forbidden,
                    CancellationToken,
                    "Forbidden",
                    instance: $"GET /staff/{user2Id}");
            };
        });
    }

    private class AllStaffEndpoints
    {
        public Func<Task> CreateStaff { get; set; } = null!;
        public Func<Task> QueryStaff { get; set; } = null!;
        public Func<Task> GetStaffById { get; set; } = null!;
    }
}