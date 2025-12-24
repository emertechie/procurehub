using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ProcureHub.WebApi.Tests.Infrastructure.Helpers;

public static class HelperExtensions
{
    public static void AssertHasError(this HttpValidationProblemDetails problemDetails, string field, string? messageContains = null)
    {
        Assert.Contains(field, problemDetails.Errors.Keys);

        if (messageContains != null)
        {
            var errorMessages = problemDetails.Errors[field];
            Assert.Contains(errorMessages, msg => msg.Contains(messageContains, StringComparison.OrdinalIgnoreCase));
        }
    }

    extension(Task<HttpResponseMessage> responseTask)
    {
        public async Task<T> ReadJsonAsync<T>(CancellationToken ct = default)
        {
            var response = await responseTask;
            return await response.ReadJsonAsync<T>(ct);
        }
    }

    extension(HttpResponseMessage response)
    {
        public Task<T> ReadJsonAsync<T>(CancellationToken ct = default)
        {
            response.EnsureSuccessStatusCode();
            return response.Content.ReadFromJsonAsync<T>(ct)!;
        }

        public async Task<ProblemDetails> AssertProblemDetailsAsync(
            HttpStatusCode expectedStatus,
            string? title = null,
            string? detail = null,
            string? instance = null,
            CancellationToken ct = default)
        {
            Assert.Equal(expectedStatus, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(ct);
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

            if (instance != null)
            {
                Assert.Equal(instance, problemDetails.Instance);
            }

            return problemDetails;
        }

        public async Task<HttpValidationProblemDetails> AssertValidationProblemAsync(
            CancellationToken ct = default,
            string? title = null,
            string? detail = null,
            IDictionary<string, string[]>? errors = null)
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

            var problemDetails = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(ct);

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
    }
}
