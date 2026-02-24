using Bunit;
using Bunit.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using ProcureHub.Application.Common;
using Radzen;

namespace ProcureHub.BlazorApp.Tests.Infrastructure;

/// <summary>
/// Base test context for bUnit Blazor component tests.
/// Pre-registers Radzen services and provides auth/handler helpers.
/// </summary>
public abstract class BlazorTestContext : BunitContext
{
    protected BunitAuthorizationContext AuthContext { get; }

    protected BlazorTestContext()
    {
        // Allow Radzen JS interop calls to pass through without explicit setup
        JSInterop.Mode = JSRuntimeMode.Loose;

        // Register Radzen services that components commonly inject
        Services.AddScoped<DialogService>();
        Services.AddScoped<NotificationService>();
        Services.AddScoped<TooltipService>();
        Services.AddScoped<ContextMenuService>();

        // Enable fake authorization support
        AuthContext = this.AddAuthorization();
    }

    /// <summary>
    /// Registers a mock IQueryHandler that returns the given response for any request.
    /// </summary>
    protected IQueryHandler<TRequest, TResponse> AddMockQueryHandler<TRequest, TResponse>(TResponse response)
    {
        var handler = Substitute.For<IQueryHandler<TRequest, TResponse>>();
        handler.HandleAsync(Arg.Any<TRequest>(), Arg.Any<CancellationToken>())
            .Returns(response);
        Services.AddSingleton(handler);
        return handler;
    }

    /// <summary>
    /// Registers a mock ICommandHandler{TCommand, TResponse} that returns the given response.
    /// </summary>
    protected ICommandHandler<TCommand, TResponse> AddMockCommandHandler<TCommand, TResponse>(TResponse response)
    {
        var handler = Substitute.For<ICommandHandler<TCommand, TResponse>>();
        handler.HandleAsync(Arg.Any<TCommand>(), Arg.Any<CancellationToken>())
            .Returns(response);
        Services.AddSingleton(handler);
        return handler;
    }

    /// <summary>
    /// Registers a mock ICommandHandler{TCommand} (void return) that completes successfully.
    /// </summary>
    protected ICommandHandler<TCommand> AddMockCommandHandler<TCommand>()
    {
        var handler = Substitute.For<ICommandHandler<TCommand>>();
        handler.HandleAsync(Arg.Any<TCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        Services.AddSingleton(handler);
        return handler;
    }

    /// <summary>
    /// Sets up auth context as an authorized user with the given roles.
    /// </summary>
    protected void AuthorizeWithRoles(params string[] roles)
    {
        AuthContext.SetAuthorized("test-user");
        AuthContext.SetRoles(roles);
    }
}
