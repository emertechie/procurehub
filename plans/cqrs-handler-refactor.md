# CQRS Handler Interface Refactoring Plan

Split `IRequestHandler` into query/command interfaces for clearer CQRS separation.

## New Interface Files

### Create `ProcureHub/Infrastructure/IQueryHandler.cs`

```csharp
namespace ProcureHub.Infrastructure;

/// <summary>Query handler - reads data, returns TResponse</summary>
public interface IQueryHandler<TRequest, TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken token);
}
```

### Create `ProcureHub/Infrastructure/ICommandHandler.cs`

```csharp
namespace ProcureHub.Infrastructure;

/// <summary>Command handler - mutates state, no return value</summary>
public interface ICommandHandler<TCommand>
{
    Task HandleAsync(TCommand command, CancellationToken token);
}

/// <summary>Command handler - mutates state, returns TResponse (e.g., created ID, Result)</summary>
public interface ICommandHandler<TCommand, TResponse>
{
    Task<TResponse> HandleAsync(TCommand command, CancellationToken token);
}
```

### Delete `ProcureHub/Infrastructure/IRequestHandler.cs`

After all usages are migrated, delete this file.

## Handler Classification

### Query Handlers (`IQueryHandler<TRequest, TResponse>`)

| Handler | Current Signature | New Signature |
|---------|-------------------|---------------|
| `QueryUsers.Handler` | `IRequestHandler<Request, PagedResult<Response>>` | `IQueryHandler<Request, PagedResult<Response>>` |
| `GetUserById.Handler` | `IRequestHandler<Request, Response?>` | `IQueryHandler<Request, Response?>` |
| `QueryDepartments.Handler` | `IRequestHandler<Request, Response[]>` | `IQueryHandler<Request, Response[]>` |
| `GetDepartmentById.Handler` | `IRequestHandler<Request, Response?>` | `IQueryHandler<Request, Response?>` |
| `QueryCategories.Handler` | `IRequestHandler<Request, Response[]>` | `IQueryHandler<Request, Response[]>` |
| `GetCategoryById.Handler` | `IRequestHandler<Request, Result<Response>>` | `IQueryHandler<Request, Result<Response>>` |
| `QueryRoles.Handler` | `IRequestHandler<Request, Role[]>` | `IQueryHandler<Request, Role[]>` |
| `QueryPurchaseRequests.Handler` | `IRequestHandler<Request, Result<PagedResult<Response>>>` | `IQueryHandler<Request, Result<PagedResult<Response>>>` |
| `GetPurchaseRequestById.Handler` | `IRequestHandler<Request, Result<Response>>` | `IQueryHandler<Request, Result<Response>>` |

### Command Handlers with Response (`ICommandHandler<TCommand, TResponse>`)

| Handler | Current Signature | New Signature |
|---------|-------------------|---------------|
| `CreateUser.Handler` | `IRequestHandler<Request, Result<string>>` | `ICommandHandler<Request, Result<string>>` |
| `UpdateUser.Handler` | `IRequestHandler<Request, Result>` | `ICommandHandler<Request, Result>` |
| `EnableUser.Handler` | `IRequestHandler<Request, Result>` | `ICommandHandler<Request, Result>` |
| `DisableUser.Handler` | `IRequestHandler<Request, Result>` | `ICommandHandler<Request, Result>` |
| `AssignUserToDepartment.Handler` | `IRequestHandler<Request, Result>` | `ICommandHandler<Request, Result>` |
| `CreateDepartment.Handler` | `IRequestHandler<Request, Result<Guid>>` | `ICommandHandler<Request, Result<Guid>>` |
| `UpdateDepartment.Handler` | `IRequestHandler<Request, Result>` | `ICommandHandler<Request, Result>` |
| `DeleteDepartment.Handler` | `IRequestHandler<Request, Result>` | `ICommandHandler<Request, Result>` |
| `CreateCategory.Handler` | `IRequestHandler<Request, Result<Guid>>` | `ICommandHandler<Request, Result<Guid>>` |
| `UpdateCategory.Handler` | `IRequestHandler<Request, Result>` | `ICommandHandler<Request, Result>` |
| `DeleteCategory.Handler` | `IRequestHandler<Request, Result>` | `ICommandHandler<Request, Result>` |
| `AssignRole.Handler` | `IRequestHandler<Request, Result>` | `ICommandHandler<Request, Result>` |
| `RemoveRole.Handler` | `IRequestHandler<Request, Result>` | `ICommandHandler<Request, Result>` |
| `CreatePurchaseRequest.Handler` | `IRequestHandler<Request, Result<Guid>>` | `ICommandHandler<Request, Result<Guid>>` |
| `UpdatePurchaseRequest.Handler` | `IRequestHandler<Request, Result>` | `ICommandHandler<Request, Result>` |
| `DeletePurchaseRequest.Handler` | `IRequestHandler<Request, Result>` | `ICommandHandler<Request, Result>` |
| `SubmitPurchaseRequest.Handler` | `IRequestHandler<Request, Result>` | `ICommandHandler<Request, Result>` |
| `ApprovePurchaseRequest.Handler` | `IRequestHandler<Request, Result>` | `ICommandHandler<Request, Result>` |
| `RejectPurchaseRequest.Handler` | `IRequestHandler<Request, Result>` | `ICommandHandler<Request, Result>` |

### Command Handlers without Response (`ICommandHandler<TCommand>`)

None currently - all commands return `Result` or `Result<T>`. Keep interface available for future use.

## Validation Decorator Updates

Update `ProcureHub/Infrastructure/ValidationRequestHandlerDecorator.cs`:

```csharp
using FluentValidation;

namespace ProcureHub.Infrastructure;

/// <summary>Validation decorator for query handlers</summary>
public class ValidationQueryHandlerDecorator<TRequest, TResponse>(
    IQueryHandler<TRequest, TResponse> inner,
    IValidator<TRequest>? validator = null
) : IQueryHandler<TRequest, TResponse>
{
    public async Task<TResponse> HandleAsync(TRequest request, CancellationToken token)
    {
        if (validator is not null)
        {
            var validationResult = await validator.ValidateAsync(request, token);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }
        return await inner.HandleAsync(request, token);
    }
}

/// <summary>Validation decorator for command handlers with response</summary>
public class ValidationCommandHandlerDecorator<TCommand, TResponse>(
    ICommandHandler<TCommand, TResponse> inner,
    IValidator<TCommand>? validator = null
) : ICommandHandler<TCommand, TResponse>
{
    public async Task<TResponse> HandleAsync(TCommand command, CancellationToken token)
    {
        if (validator is not null)
        {
            var validationResult = await validator.ValidateAsync(command, token);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }
        return await inner.HandleAsync(command, token);
    }
}

/// <summary>Validation decorator for command handlers without response</summary>
public class ValidationCommandHandlerDecorator<TCommand>(
    ICommandHandler<TCommand> inner,
    IValidator<TCommand>? validator = null
) : ICommandHandler<TCommand>
{
    public async Task HandleAsync(TCommand command, CancellationToken token)
    {
        if (validator is not null)
        {
            var validationResult = await validator.ValidateAsync(command, token);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }
        await inner.HandleAsync(command, token);
    }
}
```

## DI Registration Updates

Update `ProcureHub/Infrastructure/RequestHandlerExtensions.cs`:

```csharp
/// <summary>
/// Scans the current assembly for concrete classes implementing
/// <see cref="IQueryHandler{TRequest, TResponse}"/> or <see cref="ICommandHandler{TCommand, TResponse}"/>
/// or <see cref="ICommandHandler{TCommand}"/>
/// and registers them as transient services in the provided <paramref name="services"/> collection.
/// Also decorates all handlers with validation decorators.
/// </summary>
private static IServiceCollection AddRequestHandlers(this IServiceCollection services)
{
    var assembly = typeof(RequestHandlerExtensions).Assembly;
    var types = assembly.GetTypes()
        .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition);

    foreach (var type in types)
    {
        var interfaces = type.GetInterfaces()
            .Where(i => i.IsGenericType &&
                       (i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>) ||
                        i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>) ||
                        i.GetGenericTypeDefinition() == typeof(ICommandHandler<>)));

        foreach (var @interface in interfaces)
        {
            services.AddTransient(@interface, type);
        }
    }

    // Decorate with validation
    services.Decorate(typeof(IQueryHandler<,>), typeof(ValidationQueryHandlerDecorator<,>));
    services.Decorate(typeof(ICommandHandler<,>), typeof(ValidationCommandHandlerDecorator<,>));
    services.TryDecorate(typeof(ICommandHandler<>), typeof(ValidationCommandHandlerDecorator<>));

    return services;
}
```

## Usage Updates

### Blazor App

**`Components/Pages/Admin/Users/Index.razor`** (lines 10-12):
```razor
@inject IQueryHandler<QueryUsers.Request, PagedResult<QueryUsers.Response>> QueryUsersHandler
@inject ICommandHandler<EnableUser.Request, Result> EnableUserHandler
@inject ICommandHandler<DisableUser.Request, Result> DisableUserHandler
```

**`Components/Pages/Admin/Users/UserDialog.razor`** (lines 6-8):
```razor
@inject IQueryHandler<GetUserById.Request, GetUserById.Response?> GetUserHandler
@inject ICommandHandler<CreateUser.Request, Result<string>> CreateUserHandler
@inject ICommandHandler<UpdateUser.Request, Result> UpdateUserHandler
```

### WebApi Endpoints

Update all `[FromServices] IRequestHandler<...>` to use appropriate interface:

**`Features/Users/Endpoints.cs`**:
- `CreateUser` -> `ICommandHandler<CreateUser.Request, Result<string>>`
- `QueryUsers` -> `IQueryHandler<QueryUsers.Request, PagedResult<QueryUsers.Response>>`
- `GetUserById` -> `IQueryHandler<GetUserById.Request, GetUserById.Response?>`
- `UpdateUser` -> `ICommandHandler<UpdateUser.Request, Result>`
- `EnableUser` -> `ICommandHandler<EnableUser.Request, Result>`
- `DisableUser` -> `ICommandHandler<DisableUser.Request, Result>`
- `AssignUserToDepartment` -> `ICommandHandler<AssignUserToDepartment.Request, Result>`

**`Features/Departments/Endpoints.cs`**:
- `CreateDepartment` -> `ICommandHandler<CreateDepartment.Request, Result<Guid>>`
- `QueryDepartments` -> `IQueryHandler<QueryDepartments.Request, QueryDepartments.Response[]>`
- `GetDepartmentById` -> `IQueryHandler<GetDepartmentById.Request, GetDepartmentById.Response?>`
- `UpdateDepartment` -> `ICommandHandler<UpdateDepartment.Request, Result>`
- `DeleteDepartment` -> `ICommandHandler<DeleteDepartment.Request, Result>`

**`Features/Categories/Endpoints.cs`**:
- `CreateCategory` -> `ICommandHandler<CreateCategory.Request, Result<Guid>>`
- `QueryCategories` -> `IQueryHandler<QueryCategories.Request, QueryCategories.Response[]>`
- `GetCategoryById` -> `IQueryHandler<GetCategoryById.Request, Result<GetCategoryById.Response>>`
- `UpdateCategory` -> `ICommandHandler<UpdateCategory.Request, Result>`
- `DeleteCategory` -> `ICommandHandler<DeleteCategory.Request, Result>`

**`Features/Roles/Endpoints.cs`**:
- `QueryRoles` -> `IQueryHandler<QueryRoles.Request, QueryRoles.Role[]>`
- `AssignRole` -> `ICommandHandler<AssignRole.Request, Result>`
- `RemoveRole` -> `ICommandHandler<RemoveRole.Request, Result>`

**`Features/PurchaseRequests/Endpoints.cs`**:
- `CreatePurchaseRequest` -> `ICommandHandler<CreatePurchaseRequest.Request, Result<Guid>>`
- `QueryPurchaseRequests` -> `IQueryHandler<QueryPurchaseRequests.Request, Result<PagedResult<QueryPurchaseRequests.Response>>>`
- `GetPurchaseRequestById` -> `IQueryHandler<GetPurchaseRequestById.Request, Result<GetPurchaseRequestById.Response>>`
- `UpdatePurchaseRequest` -> `ICommandHandler<UpdatePurchaseRequest.Request, Result>`
- `SubmitPurchaseRequest` -> `ICommandHandler<SubmitPurchaseRequest.Request, Result>`
- `ApprovePurchaseRequest` -> `ICommandHandler<ApprovePurchaseRequest.Request, Result>`
- `RejectPurchaseRequest` -> `ICommandHandler<RejectPurchaseRequest.Request, Result>`
- `DeletePurchaseRequest` -> `ICommandHandler<DeletePurchaseRequest.Request, Result>`

**`Features/AGENTS.md`**: Update example code snippets to use new interfaces

## Implementation Order

1. Create `IQueryHandler.cs` with new interface
2. Create `ICommandHandler.cs` with new interfaces
3. Update `ValidationRequestHandlerDecorator.cs` with new decorators
4. Update `RequestHandlerExtensions.cs` to register new interfaces
5. Update all 28 handlers to use new interfaces (9 query + 19 command)
6. Update all WebApi endpoint usages (5 endpoint files)
7. Update all Blazor component usages (2 razor files)
8. Update `Features/AGENTS.md` documentation
9. Delete old `IRequestHandler.cs` file
10. Build and verify no errors
11. Run tests

## Files to Modify

| File | Changes |
|------|---------|
| `ProcureHub/Infrastructure/IQueryHandler.cs` | **CREATE** new file |
| `ProcureHub/Infrastructure/ICommandHandler.cs` | **CREATE** new file |
| `ProcureHub/Infrastructure/IRequestHandler.cs` | **DELETE** after migration |
| `ProcureHub/Infrastructure/ValidationRequestHandlerDecorator.cs` | Replace with new decorators |
| `ProcureHub/Infrastructure/RequestHandlerExtensions.cs` | Update DI registration |
| `ProcureHub/Features/Users/*.cs` (6 files) | Update handler interfaces |
| `ProcureHub/Features/Departments/*.cs` (5 files) | Update handler interfaces |
| `ProcureHub/Features/Categories/*.cs` (5 files) | Update handler interfaces |
| `ProcureHub/Features/Roles/*.cs` (3 files) | Update handler interfaces |
| `ProcureHub/Features/PurchaseRequests/*.cs` (9 files) | Update handler interfaces |
| `ProcureHub.WebApi/Features/*/Endpoints.cs` (5 files) | Update injected types |
| `ProcureHub.WebApi/Features/AGENTS.md` | Update docs |
| `ProcureHub.BlazorApp/Components/Pages/Admin/Users/Index.razor` | Update @inject |
| `ProcureHub.BlazorApp/Components/Pages/Admin/Users/UserDialog.razor` | Update @inject |

Total: ~38 files (2 new, 1 deleted, ~35 modified)
