using Microsoft.Extensions.DependencyInjection;
using ProcureHub.Features.PurchaseRequests.Services;
using ProcureHub.Infrastructure.Validation;

namespace ProcureHub.Infrastructure;

public static class DomainServiceExtensions
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<PurchaseRequestNumberGenerator>();
        services.AddRequestHandlers();
        services.AddSingleton<Instrumentation>();

        return services;
    }

    /// <summary>
    /// Scans the current assembly for concrete classes implementing
    /// <see cref="IQueryHandler{TRequest, TResponse}"/> or <see cref="ICommandHandler{TCommand, TResponse}"/>
    /// or <see cref="ICommandHandler{TCommand}"/>
    /// and registers them as transient services in the provided <paramref name="services"/> collection.
    /// Also decorates all handlers with validation decorators.
    /// </summary>
    /// <param name="services">The DI service collection to add handler registrations to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance, for chaining.</returns>
    private static IServiceCollection AddRequestHandlers(this IServiceCollection services)
    {
        var assembly = typeof(DomainServiceExtensions).Assembly;
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

        // Wrap all handlers with validation decorator
        services.Decorate(typeof(IQueryHandler<,>), typeof(ValidationQueryHandlerDecorator<,>));
        services.Decorate(typeof(ICommandHandler<,>), typeof(ValidationCommandHandlerDecorator<,>));
        services.TryDecorate(typeof(ICommandHandler<>), typeof(ValidationCommandHandlerDecorator<>));

        return services;
    }
}
