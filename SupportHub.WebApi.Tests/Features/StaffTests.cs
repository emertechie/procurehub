using System.Net;
using System.Net.Http.Json;
using SupportHub.Features.Staff;

namespace SupportHub.WebApi.Tests.Features;

public class StaffTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly HttpClient _client;

    public StaffTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        
        var factory = new WebApiTestFactory(testOutputHelper);
        _client = factory.CreateClient();
    }

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
    
    [Fact]
    public async Task Can_create_staff_member()
    {
        // Assert no Staff yet
        var listDeptsResp1 = await _client.GetAsync("/staff", CancellationToken);
        var staffList1 = await listDeptsResp1.AssertSuccessAndReadJsonAsync<ListStaff.Response[]>(CancellationToken);
        Assert.Equal(Array.Empty<ListStaff.Response>(), staffList1);

        // Create staff
        var createStaffReq = new CreateStaff.Request("new-staff1@example.com", "New Staff1");
        var createDeptResp = await _client.PostAsync("/staff", JsonContent.Create(createStaffReq), CancellationToken);
        Assert.Equal(HttpStatusCode.Created, createDeptResp.StatusCode);

        // Extract staff ID from Location header
        var location = createDeptResp.Headers.Location?.ToString();
        Assert.Matches(@"^/staff/[0-9a-fA-F-]+$", location);
        var newStaffId = location!.Split('/').Last();

        // Assert department returned in list
        var listDeptsResp2 = await _client.GetAsync("/staff", CancellationToken);
        var staffList2 = await listDeptsResp2.AssertSuccessAndReadJsonAsync<ListStaff.Response[]>(CancellationToken);
        Assert.Equal(
            new ListStaff.Response[]
            {
                new(newStaffId, "new-staff1@example.com", null, null)
            },
            staffList2);

        // Assert can get staff by ID
        var getStaffResp = await _client.GetAsync($"/staff/{newStaffId}", CancellationToken);
        var staff = await getStaffResp.AssertSuccessAndReadJsonAsync<GetStaff.Response>(CancellationToken);
        Assert.Equal(
            new GetStaff.Response(newStaffId, "new-staff1@example.com", null, null),
            staff);
    }

    [Fact]
    public async Task Cannot_create_staff_member_with_duplicate_email()
    {
        var staffEmail = "new-staff1@example.com";
        
        // Create staff 1 - should work 
        var createStaffReq1 = new CreateStaff.Request(staffEmail, "New Staff1");
        var createDeptResp1 = await _client.PostAsync("/staff", JsonContent.Create(createStaffReq1), CancellationToken);
        Assert.Equal(HttpStatusCode.Created, createDeptResp1.StatusCode);
        
        // Create staff 2 with same email - should fail 
        var createStaffReq2 = new CreateStaff.Request(staffEmail, "New Staff2");
        var createStaffResp2 = await _client.PostAsync("/staff", JsonContent.Create(createStaffReq2), CancellationToken);
        
        await createStaffResp2.AssertValidationProblemAsync(CancellationToken,
            title: "Failed to create staff user account",
            detail: "Staff.UserCreationFailed",
            errors: new Dictionary<string, string[]>
            {
                ["DuplicateUserName"] = [$"Username '{staffEmail}' is already taken."]
            });
    }

    [Fact]
    public async Task Only_admin_role_can_create_staff()
    {
        throw new NotImplementedException();
    }
}