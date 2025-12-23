using System.Net;
using System.Net.Http.Json;
using ProcureHub.Features.Departments;
using ProcureHub.WebApi.Tests.Infrastructure;
using ProcureHub.WebApi.Tests.Infrastructure.BaseTestTypes;
using ProcureHub.WebApi.Tests.Infrastructure.Xunit;

namespace ProcureHub.WebApi.Tests.Features;

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
            .ReadJsonAsync<ListDepartments.Response[]>();
        Assert.Empty(departments1);

        // Create department
        var createDeptReq = new CreateDepartment.Request("New Department");
        var createDeptResp = await HttpClient.PostAsync("/departments", JsonContent.Create(createDeptReq));
        Assert.Equal(HttpStatusCode.Created, createDeptResp.StatusCode);

        // Extract department ID from Location header
        var location = createDeptResp.Headers.Location?.ToString();
        Assert.Matches(@"^/departments/\d+$", location);
        var newDepartmentId = int.Parse(location!.Split('/').Last());

        // Assert department returned in list
        var departments2 = await HttpClient.GetAsync("/departments")
            .ReadJsonAsync<ListDepartments.Response[]>();
        Assert.Equal(
            new ListDepartments.Response[] { new(newDepartmentId, "New Department") },
            departments2);

        // Assert can get department by ID
        var department = await HttpClient.GetAsync($"/departments/{newDepartmentId}")
            .ReadJsonAsync<GetDepartment.Response>();
        Assert.Equal(
            new GetDepartment.Response(newDepartmentId, "New Department"),
            department);
    }
}
