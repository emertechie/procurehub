using Microsoft.Extensions.DependencyInjection;

namespace ProcureHub.Application.Common;

public class RequestDispatcher(IServiceProvider serviceProvider) : IRequestDispatcher
{
    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> req, CancellationToken ct) =>
        serviceProvider.GetRequiredService<IRequestHandler<IRequest<TResponse>, TResponse>>()
            .HandleAsync(req, ct);
}
