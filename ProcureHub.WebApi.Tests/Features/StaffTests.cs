using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity.Data;
using ProcureHub.Features.Staff;
using ProcureHub.WebApi.ApiResponses;

namespace ProcureHub.WebApi.Tests.Features;

public class StaffTests(ITestOutputHelper testOutputHelper)
    : IntegrationTestsBase(testOutputHelper)
{
    private const string ValidStaffEmail = "staff1@example.com";
    private const string ValidStaffPassword = "Test1234!";

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
        var staffList1 = await listStaffResp1.AssertSuccessAndReadJsonAsync<ApiPagedResponse<ListStaff.Response>>(CancellationToken);
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
        var staffList2 = await listStaffResp2.AssertSuccessAndReadJsonAsync<ApiPagedResponse<ListStaff.Response>>(CancellationToken);
        var newStaff = Assert.Single(staffList2!.Data);
        Assert.Equal(newUserId, newStaff.Id);
        Assert.Equal(newUserEmailLower, newStaff.Email);
        
        // Can get new Staff by ID
        var getStaffResp = await HttpClient.GetAsync($"/staff/{newUserId}", CancellationToken);
        var staffById = await getStaffResp.AssertSuccessAndReadJsonAsync<ApiDataResponse<GetStaff.Response>>(CancellationToken);
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
        var staffList2 = await listStaffResp2.AssertSuccessAndReadJsonAsync<ApiPagedResponse<ListStaff.Response>>(CancellationToken);
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
    public async Task Staff_member_can_read_own_staff_record_by_id()
    {
        throw new NotImplementedException();
    }
    
    [Fact]
    public async Task Staff_member_cannot_get_other_staff_record_by_id()
    {
        const string email1 = "staff1@example.com";
        const string email2 = "staff2@example.com";

        // Log in as admin to be able to manage staff
        await LoginAsAdminAsync();

        // Admin creates 2 Staff users
        var newStaffReq1 = JsonContent.Create(new { email = email1, password = ValidStaffPassword });
        var regResp1 = await HttpClient.PostAsync("/staff", newStaffReq1, CancellationToken);
        Assert.Equal(HttpStatusCode.Created, regResp1.StatusCode);

        var newStaffReq2 = JsonContent.Create(new { email = email2, password = ValidStaffPassword });
        var regResp2 = await HttpClient.PostAsync("/staff", newStaffReq2, CancellationToken);
        Assert.Equal(HttpStatusCode.Created, regResp2.StatusCode);

        // Extract user 2's ID from Location header
        var location = regResp2.Headers.Location?.ToString();
        var user2Id = location!.Split('/').Last();

        // Log in as Staff member 1
        await LoginAsync(email1, ValidStaffPassword);

        // Try to get staff member 2 by ID - should *fail*
        var getStaffResp = await HttpClient.GetAsync($"/staff/{user2Id}", CancellationToken);
        await getStaffResp.AssertProblemDetailsAsync(
            HttpStatusCode.Forbidden,
            CancellationToken,
            title: "Forbidden",
            instance: $"GET /staff/{user2Id}");
    }

    [Fact]
    public async Task Staff_member_cannot_list_all_staff()
    {
        const string email = "staff1@example.com";

        // Log in as admin to be able to manage staff
        await LoginAsAdminAsync();

        // Admin creates Staff user
        var newStaffReq = JsonContent.Create(new { email, password = ValidStaffPassword });
        var regResp = await HttpClient.PostAsync("/staff", newStaffReq, CancellationToken);
        Assert.Equal(HttpStatusCode.Created, regResp.StatusCode);

        // Log in as Staff member
        await LoginAsync(email, ValidStaffPassword);

        // Try to list all staff - should fail (don't have "Admin" role)
        var listStaffResp = await HttpClient.GetAsync("/staff", CancellationToken);
        await listStaffResp.AssertProblemDetailsAsync(
            HttpStatusCode.Forbidden,
            CancellationToken,
            title: "Forbidden",
            instance: "GET /staff");
    }

    [Fact]
    public async Task Staff_member_cannot_create_another_staff_member()
    {
        const string email1 = "staff1@example.com";
        const string email2 = "staff2@example.com";
            
        // Log in as admin to be able to manage staff
        await LoginAsAdminAsync();

        // Admin creates Staff user
        var newStaffReq1 = JsonContent.Create(new { email = email1, password = ValidStaffPassword });
        var regResp1 = await HttpClient.PostAsync("/staff", newStaffReq1, CancellationToken);
        Assert.Equal(HttpStatusCode.Created, regResp1.StatusCode);
            
        // Log in as Staff member
        await LoginAsync(email1, ValidStaffPassword);

        // Try to create another staff member - should *fail*
        var newStaffReq = JsonContent.Create(new { email = email2, password = ValidStaffPassword });
        var regResp = await HttpClient.PostAsync("/staff", newStaffReq, CancellationToken);
        await regResp.AssertProblemDetailsAsync(
            HttpStatusCode.Forbidden,
            CancellationToken,
            title: "Forbidden",
            instance: "POST /staff");
    }
}