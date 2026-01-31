using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace ProcureHub.Infrastructure.Hosting;

public static class HostingHealthEndpointExtensions
{
    public static IEndpointConventionBuilder MapLivenessHealthEndpoint(
        this IEndpointRouteBuilder app,
        string path = "/health")
    {
        return app.MapHealthChecks(path, new HealthCheckOptions
        {
            Predicate = _ => false // Don't run any checks, just return healthy if app is running
        });
    }

    public static IEndpointConventionBuilder MapReadinessHealthEndpoint(
        this IEndpointRouteBuilder app,
        string path = "/health/ready")
    {
        return app.MapHealthChecks(path);
    }
}
