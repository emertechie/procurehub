using System.Net;
using System.Net.Http.Json;
using SupportHub.Features.Departments;

namespace SupportHub.WebApi.Tests;

public class DepartmentTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly HttpClient _client;

    public DepartmentTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        
        var factory = new WebApiTestFactory(testOutputHelper);
        _client = factory.CreateClient();
    }

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
    
    [Fact]
    public async Task Can_create_and_fetch_department()
    {
        // Assert no departments yet
        var listDeptsResp1 = await _client.GetAsync("/departments", CancellationToken);
        var departments1 = await listDeptsResp1.Content.ReadFromJsonAsync<ListDepartments.Response[]>(CancellationToken);
        Assert.Equal(Array.Empty<ListDepartments.Response>(), departments1);

        // Create department
        var createDeptReq = new CreateDepartment.Request("New Department");
        var createDeptResp = await _client.PostAsync("/departments", JsonContent.Create(createDeptReq), CancellationToken);
        Assert.Equal(HttpStatusCode.Created, createDeptResp.StatusCode);

        // Extract department ID from Location header
        var location = createDeptResp.Headers.Location?.ToString();
        Assert.Matches(@"^/departments/\d+$", location);
        var newDepartmentId = int.Parse(location!.Split('/').Last());
        
        // Assert department returned in list
        var listDeptsResp2 = await _client.GetAsync("/departments", CancellationToken);
        var departments2 = await listDeptsResp2.Content.ReadFromJsonAsync<ListDepartments.Response[]>(CancellationToken);
        Assert.Equal(
            new ListDepartments.Response[] { new(newDepartmentId, "New Department") },
            departments2);
        
        // Assert can get department by ID
        var getDeptResp = await _client.GetAsync($"/departments/{newDepartmentId}", CancellationToken);
        var department = await getDeptResp.Content.ReadFromJsonAsync<GetDepartment.Response>(CancellationToken);
        Assert.Equal(
            new GetDepartment.Response(newDepartmentId, "New Department"),
            department);
    }

}
