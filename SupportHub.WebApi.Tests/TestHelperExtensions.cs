using System.Net;
using System.Net.Http.Json;

namespace SupportHub.WebApi.Tests;

public static class TestHelperExtensions
{
    public static Task<T?> AssertSuccessAndReadJsonAsync<T>(this HttpResponseMessage response, CancellationToken token)
    {
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return response.Content.ReadFromJsonAsync<T>(token);
    }
}
