- Follow this guide when implementing endpoint configuration types in this folder
- Use `MapGroup` at top to apply common concerns like auth, FluentValidation, and defining OpenAPI tags. Example:
```cs
    public static void ConfigureUsersEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("")
            .RequireAuthorization(AuthorizationPolicyNames.Authenticated, RolePolicyNames.AdminOnly)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .AddFluentValidationAutoValidation()
            .WithTags("Users");

        group.MapPost("/users", async (
                [FromServices] IRequestHandler<CreateUser.Request, Result<string>> handler,
                [FromBody] CreateUser.Request request,
                CancellationToken token
            ) =>
            {
                var result = await handler.HandleAsync(request, token);
                return result.Match(
                    newUserId => Results.Created($"/users/{newUserId}", new { userId = newUserId }),
                    error => error.ToProblemDetails()
                );
            })
            .WithName("CreateUsers")
            .Produces<string>(StatusCodes.Status201Created)
            .ProducesValidationProblem();
```
- When mapping an endpoint, order lambda arguments as follows: [FromServices], [FromBody], CancellationToken, (other parameters). Example:
```cs
        group.MapPut("/users/{id}", async (
                [FromServices] IRequestHandler<UpdateUser.Request, Result> handler,
                [FromBody] UpdateUser.Request request,
                CancellationToken token,
                string id
            ) =>
```
- Each endpoint must assign an operation name using `.WithName`
- Each endpoint must define all possible return values using `.Produces` calls. Example: `.ProducesValidationProblem()`, `.Produces<GetUserById.Response>()`, etc
- If an `IRequestHandler` returns a `PagedResult<T>`, use `PagedResponse.From(pagedResult);` to return result from endpoint handler.
- Good example to follow: `ProcureHub.WebApi/Features/Users/Endpoints.cs`
