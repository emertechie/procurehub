using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SupportHub.WebApi.Tests;

public static class HttpResponseAssertions
{
    public static async Task<ProblemDetails> AssertProblemDetailsAsync(this HttpResponseMessage response,
        HttpStatusCode expectedStatus,
        CancellationToken cancellationToken = default,
        string? title = null,
        string? detail = null)
    {
        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        Assert.NotNull(problemDetails);
        Assert.Equal((int)expectedStatus, problemDetails.Status);
        
        if (detail != null)
        {
            Assert.Equal(detail, problemDetails.Detail);
        }

        if (title != null)
        {
            Assert.Equal(title, problemDetails.Title);
        }

        return problemDetails;
    }

    public static async Task<HttpValidationProblemDetails> AssertValidationProblemAsync(
        this HttpResponseMessage response,
        CancellationToken cancellationToken = default,
        string? title = null,
        string? detail = null,
        IDictionary<string, string[]>? errors = null)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problemDetails = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(cancellationToken);

        Assert.NotNull(problemDetails);
        Assert.Equal(400, problemDetails.Status);
        Assert.NotNull(problemDetails.Errors);
        Assert.NotEmpty(problemDetails.Errors);

        if (detail != null)
        {
            Assert.Equal(detail, problemDetails.Detail);
        }

        if (title != null)
        {
            Assert.Equal(title, problemDetails.Title);
        }

        if (errors != null)
        {
            Assert.Equal(errors, problemDetails.Errors);
        }

        return problemDetails;
    }

    public static void AssertHasError(this HttpValidationProblemDetails problemDetails, string field, string? messageContains = null)
    {
        Assert.Contains(field, problemDetails.Errors.Keys);
        
        if (messageContains != null)
        {
            var errorMessages = problemDetails.Errors[field];
            Assert.Contains(errorMessages, msg => msg.Contains(messageContains, StringComparison.OrdinalIgnoreCase));
        }
    }
}