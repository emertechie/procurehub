using System.Net;

namespace SupportHub.WebApi.Tests;

public class DepartmentTests
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _testOutputHelper;

    public DepartmentTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        var factory = new WebApiTestFactory(testOutputHelper);
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Can_create_department()
    {
        _testOutputHelper.WriteLine("*** Running test: Can_create_department");
        
        // Act
        var response = await _client.GetAsync("/weatherforecast", TestContext.Current.CancellationToken);

        // TODO: output response

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        throw new NotImplementedException();
    }
}
