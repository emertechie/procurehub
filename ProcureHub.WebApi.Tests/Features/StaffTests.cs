using System.Net;
using System.Net.Http.Json;
using ProcureHub.Features.Staff;

namespace ProcureHub.WebApi.Tests.Features;

public class StaffTests(ITestOutputHelper testOutputHelper)
    : TestsBase(testOutputHelper)
{
    private const string ValidStaffEmail = "staff1@example.com";

    /// <summary>
    /// Note: In a real-world system, this would use invite emails and a time-limited token.
    /// Keeping it simple here for demo.
    /// </summary>
    [Fact]
    public async Task Admin_user_can_create_staff_entity()
    {
        // Log in as admin to be able to query staff
        await LoginAsAdminAsync();

        var newUserEmailMixedCase = "Staff1@Example.COM";
        var newUserEmailLower = newUserEmailMixedCase.ToLowerInvariant();

        // Search for new Staff by email -> Not found
        var listStaffResp1 = await Client.GetAsync($"/staff?email={newUserEmailMixedCase}", CancellationToken);
        var staffList1 = await listStaffResp1.AssertSuccessAndReadJsonAsync<object[]>(CancellationToken);
        Assert.Empty(staffList1!);

        // Admin creates user
        var newStaffReq = JsonContent.Create(new { email = newUserEmailMixedCase, password = "Test1234!" });
        var regResp = await Client.PostAsync("/staff", newStaffReq, CancellationToken);
        Assert.Equal(HttpStatusCode.Created, regResp.StatusCode);
        
        // Ensure Location header contains new user ID
        var location = regResp.Headers.Location?.ToString();
        Assert.Matches(@"^/staff/[0-9a-f-]+$", location);
        var newUserId = location!.Split('/').Last();

        // Search for new Staff by email -> Found
        var listStaffResp2 = await Client.GetAsync($"/staff?email={newUserEmailMixedCase}", CancellationToken);
        var staffList2 = await listStaffResp2.AssertSuccessAndReadJsonAsync<ListStaff.Response[]>(CancellationToken);
        var newStaff = Assert.Single(staffList2!);
        Assert.Equal(newUserId, newStaff.Id);
        Assert.Equal(newUserEmailLower, newStaff.Email);
        
        // Can get new Staff by ID
        var getStaffResp = await Client.GetAsync($"/staff/{newUserId}", CancellationToken);
        var staffById = await getStaffResp.AssertSuccessAndReadJsonAsync<GetStaff.Response>(CancellationToken);
        Assert.Equal(newUserId, staffById!.Id);
        Assert.Equal(newUserEmailLower, staffById!.Email);
    }

    [Fact]
    public async Task Staff_member_can_login()
    {
        throw new NotImplementedException();
    }

    public class RoleTests(ITestOutputHelper testOutputHelper)
        : TestsBase(testOutputHelper)
    {
        [Fact]
        public async Task Staff_member_cannot_create_another_staff_member()
        {
            // TODO: ensures Admin role only
            throw new NotImplementedException();
        }

        [Fact]
        public async Task Staff_member_cannot_list_all_staff()
        {
            // TODO: ensures Admin role only
            throw new NotImplementedException();
        }

        [Fact]
        public async Task Staff_member_cannot_get_other_staff_by_id()
        {
            // TODO: ensures Admin role only
            throw new NotImplementedException();
        }
    }

    [Fact]
    public async Task Cannot_create_staff_member_with_duplicate_email()
    {
        // Log in as admin to be able to query staff
        await LoginAsAdminAsync();
        
        // Register call 1 - should work
        var regRequest1 = JsonContent.Create(new { email = ValidStaffEmail, password = "Test1234!" });
        var regResp1 = await Client.PostAsync("/register", regRequest1, CancellationToken);
        Assert.Equal(HttpStatusCode.OK, regResp1.StatusCode);
        
        // Assert only 1 Staff record
        var listStaffResp1 = await Client.GetAsync("/staff", CancellationToken);
        var staffList1 = await listStaffResp1.AssertSuccessAndReadJsonAsync<ListStaff.Response[]>(CancellationToken);
        var newStaff1 = Assert.Single(staffList1!);
        Assert.Equal(ValidStaffEmail, newStaff1.Email);
        
        // Create call 2 - should fail 
        var regRequest2 = JsonContent.Create(new { email = ValidStaffEmail, password = "Test1234!" });
        var regResp2 = await Client.PostAsync("/register", regRequest2, CancellationToken);
        await regResp2.AssertValidationProblemAsync(CancellationToken,
            errors: new Dictionary<string, string[]>
            {
                ["DuplicateUserName"] = [$"Username '{ValidStaffEmail}' is already taken."]
            });

        // Assert still only 1 Staff record
        var listStaffResp2 = await Client.GetAsync("/staff", CancellationToken);
        var staffList2 = await listStaffResp2.AssertSuccessAndReadJsonAsync<ListStaff.Response[]>(CancellationToken);
        var newStaff2 = Assert.Single(staffList2!);
        Assert.Equal(ValidStaffEmail, newStaff2.Email);
    }
}