using System.Collections.Concurrent;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;

namespace ProcureHub.Application.Common;

public class RequestDispatcher(IServiceProvider serviceProvider) : IRequestDispatcher
{
    private static readonly ConcurrentDictionary<(Type RequestType, Type ResponseType),
        Func<IServiceProvider, object, CancellationToken, object>> InvokerCache = new();

    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();
        var responseType = typeof(TResponse);
        var invoker = InvokerCache.GetOrAdd(
            (requestType, responseType),
            static key => CreateInvoker(key.RequestType, key.ResponseType));

        return (Task<TResponse>)invoker(serviceProvider, request, cancellationToken);
    }

    private static Func<IServiceProvider, object, CancellationToken, object> CreateInvoker(Type requestType, Type responseType)
    {
        var serviceProviderParameter = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");
        var requestParameter = Expression.Parameter(typeof(object), "request");
        var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
        var getRequiredServiceCall = Expression.Call(
            typeof(ServiceProviderServiceExtensions),
            nameof(ServiceProviderServiceExtensions.GetRequiredService),
            Type.EmptyTypes,
            serviceProviderParameter,
            Expression.Constant(handlerType, typeof(Type))
        );

        var handlerCast = Expression.Convert(getRequiredServiceCall, handlerType);
        var requestCast = Expression.Convert(requestParameter, requestType);
        var handleAsyncMethod = handlerType.GetMethod(nameof(IRequestHandler<IRequest<object>, object>.HandleAsync))!;
        var handleCall = Expression.Call(handlerCast, handleAsyncMethod, requestCast, cancellationTokenParameter);
        var boxedTask = Expression.Convert(handleCall, typeof(object));

        return Expression.Lambda<Func<IServiceProvider, object, CancellationToken, object>>(
            boxedTask,
            serviceProviderParameter,
            requestParameter,
            cancellationTokenParameter
        ).Compile();
    }
}
