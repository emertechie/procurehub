using System.Net;
using System.Net.Http.Json;

namespace ProcureHub.WebApi.Tests;

public static class TestHelpers
{
    public static Task<T?> AssertSuccessAndReadJsonAsync<T>(this HttpResponseMessage response, CancellationToken token)
    {
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return response.Content.ReadFromJsonAsync<T>(token);
    }
}