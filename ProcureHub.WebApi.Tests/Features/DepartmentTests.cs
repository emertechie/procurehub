using System.Net;
using System.Net.Http.Json;
using ProcureHub.Features.Departments;

namespace ProcureHub.WebApi.Tests.Features;

public class DepartmentTests(ITestOutputHelper testOutputHelper)
    : DbTestsBase(testOutputHelper)
{
    [Fact]
    public async Task Can_create_and_fetch_department()
    {
        // Assert no departments yet
        var listDeptsResp1 = await Client.GetAsync("/departments", CancellationToken);
        var departments1 = await listDeptsResp1.AssertSuccessAndReadJsonAsync<ListDepartments.Response[]>(CancellationToken);
        Assert.Equal(Array.Empty<ListDepartments.Response>(), departments1);

        // Create department
        var createDeptReq = new CreateDepartment.Request("New Department");
        var createDeptResp = await Client.PostAsync("/departments", JsonContent.Create(createDeptReq), CancellationToken);
        Assert.Equal(HttpStatusCode.Created, createDeptResp.StatusCode);

        // Extract department ID from Location header
        var location = createDeptResp.Headers.Location?.ToString();
        Assert.Matches(@"^/departments/\d+$", location);
        var newDepartmentId = int.Parse(location!.Split('/').Last());
        
        // Assert department returned in list
        var listDeptsResp2 = await Client.GetAsync("/departments", CancellationToken);
        var departments2 = await listDeptsResp2.AssertSuccessAndReadJsonAsync<ListDepartments.Response[]>(CancellationToken);
        Assert.Equal(
            new ListDepartments.Response[] { new(newDepartmentId, "New Department") },
            departments2);
        
        // Assert can get department by ID
        var getDeptResp = await Client.GetAsync($"/departments/{newDepartmentId}", CancellationToken);
        var department = await getDeptResp.AssertSuccessAndReadJsonAsync<GetDepartment.Response>(CancellationToken);
        Assert.Equal(
            new GetDepartment.Response(newDepartmentId, "New Department"),
            department);
    }

    [Fact]
    public async Task Can_assign_staff_to_department()
    {
        // Set up departments
        await Task.WhenAll(CreateDepartment("Sales"), CreateDepartment("Engineering"));

        // Set up Staff

        throw new NotImplementedException();
    }
    
    private Task CreateDepartment(string name)
        => Client.PostAsync("/departments", JsonContent.Create(new CreateDepartment.Request(name)), CancellationToken);
}
