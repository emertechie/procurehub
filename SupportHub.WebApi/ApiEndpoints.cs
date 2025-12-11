using Microsoft.AspNetCore.Mvc;
using SupportHub.Common;
using SupportHub.Features.Departments;
using SupportHub.Features.Staff;
using SupportHub.Infrastructure;

namespace SupportHub.WebApi;

public static class ApiEndpoints
{
    public static void Configure(WebApplication app)
    {
        ConfigureStaffEndpoints(app);
        ConfigureDepartmentEndpoints(app);
    }

    private static void ConfigureStaffEndpoints(WebApplication app)
    {
        // Staff
        app.MapGet("/staff", (
                [FromServices] IRequestHandler<ListStaff.Request, ListStaff.Response[]> handler,
                CancellationToken token
            ) => handler.HandleAsync(new ListStaff.Request(), token))
            .WithName("GetStaff")
            .WithTags("Staff");

        app.MapGet("/staff/{id}", async (
                string id,
                [FromServices] IRequestHandler<GetStaff.Request, GetStaff.Response?> handler,
                CancellationToken token) => await handler.HandleAsync(new GetStaff.Request(id), token) is var response
                ? Results.Ok(response)
                : Results.NotFound())
            .WithName("GetStaffById")
            .WithTags("Staff");

        app.MapPost("/staff", async (
                CreateStaff.Request request,
                [FromServices] IRequestHandler<CreateStaff.Request, Result<string>> handler,
                CancellationToken token
            ) =>
            {
                var result = await handler.HandleAsync(request, token);
                return result.Match(
                    userId => Results.Created($"/staff/{userId}", new { userId }),
                    error => error.ToProblemDetails()
                );
            })
            .WithName("CreateStaff")
            .WithTags("Staff")
            .Produces<object>(StatusCodes.Status201Created)
            .ProducesValidationProblem();
    }

    private static void ConfigureDepartmentEndpoints(WebApplication app)
    {
        // Departments
        app.MapGet("/departments", (
                [FromServices] IRequestHandler<ListDepartments.Request, ListDepartments.Response[]> handler,
                CancellationToken token
            ) => handler.HandleAsync(new ListDepartments.Request(), token))
            .WithName("GetDepartments")
            .WithTags("Departments");

        app.MapGet("/departments/{id:int}", async (
                    int id,
                    [FromServices] IRequestHandler<GetDepartment.Request, GetDepartment.Response?> handler,
                    CancellationToken token) =>
                await handler.HandleAsync(new GetDepartment.Request(id), token) is var response
                    ? Results.Ok(response)
                    : Results.NotFound())
            .WithName("GetDepartmentById")
            .WithTags("Departments");

        app.MapPost("/departments", async (
                CreateDepartment.Request request,
                [FromServices] IRequestHandler<CreateDepartment.Request, int> handler,
                CancellationToken token
            ) =>
            {
                var newId = await handler.HandleAsync(request, token);
                return Results.Created($"/departments/{newId}", null);
            })
            .WithName("CreateDepartment")
            .WithTags("Departments");
    }
}