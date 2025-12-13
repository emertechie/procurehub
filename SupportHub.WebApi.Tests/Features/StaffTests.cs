using System.Net;
using System.Net.Http.Json;
using SupportHub.Features.Staff;
using SupportHub.Features.Staff.Registration;

namespace SupportHub.WebApi.Tests.Features;

public class StaffTests(ITestOutputHelper testOutputHelper)
    : TestsBase(testOutputHelper)
{
    private const string ValidStaffEmail = "staff1@example.com";

    [Fact]
    public async Task Can_register_user_with_an_approved_email_and_create_a_linked_Staff_entity()
    {
        // Log in as admin to be able to query staff
        await LoginAsAdminAsync();

        // Assert no existing Staff entities
        var listStaffResp1 = await Client.GetAsync("/staff", CancellationToken);
        var staffList1 = await listStaffResp1.AssertSuccessAndReadJsonAsync<object[]>(CancellationToken);
        Assert.Empty(staffList1!);

        var newUserEmail = "staff1@example.com";

        // Note: currently using a dummy validator implementation. Will need to update
        Assert.Contains(newUserEmail, DummyStaffRegistrationValidator.AllowedEmails);
        
        // Register with known email
        var regRequest = JsonContent.Create(new { email = newUserEmail, password = "Test1234!" });
        var regResp = await Client.PostAsync("/register", regRequest, CancellationToken);
        Assert.Equal(HttpStatusCode.OK, regResp.StatusCode);

        // Get new Staff included in Staff list 
        var listStaffResp2 = await Client.GetAsync("/staff", CancellationToken);
        var staffList2 = await listStaffResp2.AssertSuccessAndReadJsonAsync<ListStaff.Response[]>(CancellationToken);
        var newStaff = Assert.Single(staffList2!);
        Assert.Equal(newUserEmail, newStaff.Email);
        
        // Get new Staff by ID
        var getStaffResp = await Client.GetAsync($"/staff/{newStaff.Id}", CancellationToken);
        var staffById = await getStaffResp.AssertSuccessAndReadJsonAsync<GetStaff.Response>(CancellationToken);
        Assert.Equal(newUserEmail, staffById!.Email);
    }
    
    [Fact]
    public async Task Cannot_register_staff_member_with_duplicate_email()
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