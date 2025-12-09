using Microsoft.Extensions.DependencyInjection;

namespace SupportHub.Infrastructure;

public static class RequestHandlerExtensions
{
    /// <summary>
    /// Scans the current assembly for concrete classes implementing
    /// <see cref="IRequestHandler{TRequest}"/> or <see cref="IRequestHandler{TRequest, TResponse}"/>
    /// and registers them as transient services in the provided <paramref name="services"/> collection.
    /// </summary>
    /// <param name="services">The DI service collection to add handler registrations to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance, for chaining.</returns>
    public static IServiceCollection AddRequestHandlers(this IServiceCollection services)
    {
        var assembly = typeof(RequestHandlerExtensions).Assembly;
        var types = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition);

        foreach (var type in types)
        {
            var interfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType &&
                           (i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) ||
                            i.GetGenericTypeDefinition() == typeof(IRequestHandler<>)));

            foreach (var @interface in interfaces)
            {
                services.AddTransient(@interface, type);
            }
        }

        return services;
    }
}