using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ProcureHub.Infrastructure.Hosting;

// Adds common Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";

    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter("ProcureHub"); // Custom Meter from ProcureHub.Metrics
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName)
                    .AddSource("ProcureHub") // Custom ActivitySource from ProcureHub.Instrumentation
                    .AddAspNetCoreInstrumentation(aspNetTracing =>
                        // Exclude health check requests from tracing
                        aspNetTracing.Filter = context =>
                            !context.Request.Path.StartsWithSegments(HealthEndpointPath)
                            && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath)
                    )
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation();
            })
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: builder.Environment.ApplicationName,
                    serviceVersion: typeof(ApplicationDbContext).Assembly.GetName().Version?.ToString() ?? "unknown",
                    serviceInstanceId: Environment.MachineName));

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        // Per-signal OTLP endpoints allow sending logs, traces, and metrics to different backends.
        // Signal-specific env vars take full URLs (no path auto-appending).
        //
        // Example config for Victoria stack:
        //   OTEL_EXPORTER_OTLP_LOGS_ENDPOINT=http://localhost:9428/insert/opentelemetry/v1/logs
        //   OTEL_EXPORTER_OTLP_TRACES_ENDPOINT=http://localhost:10428/insert/opentelemetry/v1/traces
        //   OTEL_EXPORTER_OTLP_METRICS_ENDPOINT=http://localhost:8428/opentelemetry/v1/metrics
        //   OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf
        //
        // Example config for Seq (single endpoint for all signals):
        //   OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:5341/ingest/otlp
        //   OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf
        //
        // Example config for Aspire dashboard (single endpoint, gRPC):
        //   OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317

        var logsEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_LOGS_ENDPOINT"];
        var tracesEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_TRACES_ENDPOINT"];
        var metricsEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_METRICS_ENDPOINT"];
        var hasPerSignalEndpoints = !string.IsNullOrWhiteSpace(logsEndpoint)
            || !string.IsNullOrWhiteSpace(tracesEndpoint)
            || !string.IsNullOrWhiteSpace(metricsEndpoint);

        if (hasPerSignalEndpoints)
        {
            // Use per-signal exporters when any signal-specific endpoint is configured.
            // This supports scenarios where each signal goes to a different backend (e.g. Victoria stack).
            var protocol = ParseOtlpProtocol(builder.Configuration["OTEL_EXPORTER_OTLP_PROTOCOL"]);

            if (!string.IsNullOrWhiteSpace(logsEndpoint))
            {
                builder.Logging.AddOpenTelemetry(logging =>
                    logging.AddOtlpExporter("logs", options =>
                    {
                        options.Endpoint = new Uri(logsEndpoint);
                        options.Protocol = protocol;
                    }));
            }

            if (!string.IsNullOrWhiteSpace(tracesEndpoint))
            {
                builder.Services.AddOpenTelemetry()
                    .WithTracing(tracing => tracing.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(tracesEndpoint);
                        options.Protocol = protocol;
                    }));
            }

            if (!string.IsNullOrWhiteSpace(metricsEndpoint))
            {
                builder.Services.AddOpenTelemetry()
                    .WithMetrics(metrics => metrics.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(metricsEndpoint);
                        options.Protocol = protocol;
                    }));
            }
        }
        else
        {
            // Fall back to UseOtlpExporter() which sends all signals to the same base endpoint.
            var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
            if (useOtlpExporter)
            {
                builder.Services.AddOpenTelemetry()
                    .UseOtlpExporter();
            }
        }

        return builder;
    }

    private static OtlpExportProtocol ParseOtlpProtocol(string? protocol)
    {
        return protocol switch
        {
            "http/protobuf" => OtlpExportProtocol.HttpProtobuf,
            "grpc" => OtlpExportProtocol.Grpc,
            _ => OtlpExportProtocol.HttpProtobuf
        };
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Adding health checks endpoints to applications in non-development environments has security implications.
        // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
        if (app.Environment.IsDevelopment())
        {
            // All health checks must pass for app to be considered ready to accept traffic after starting
            app.MapHealthChecks(HealthEndpointPath)
                .ExcludeFromDescription();

            // Only health checks tagged with the "live" tag must pass for app to be considered alive
            app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            }).ExcludeFromDescription();
        }

        return app;
    }
}
