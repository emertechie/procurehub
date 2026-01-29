using Microsoft.AspNetCore.Mvc;

namespace ProcureHub.WebApi.Infrastructure;

/// <summary>
/// Centralizes customization of ProblemDetails responses to ensure consistent formatting.
/// </summary>
public static class ProblemDetailsHelper
{
    /// <summary>
    /// Adds standard extensions (Instance, requestId) to a ProblemDetails response.
    /// </summary>
    public static void ExtendWithHttpContext(ProblemDetails problemDetails, HttpContext httpContext)
    {
        problemDetails.Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}";
        problemDetails.Extensions.TryAdd("requestId", httpContext.TraceIdentifier);
    }
}
