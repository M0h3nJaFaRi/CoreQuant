using Common.Application.Cqrs.Abstractions;
using Common.Application.Cqrs.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Application.Cqrs;

/// <summary>
/// Default <see cref="IQueryBus"/> resolving handlers and ordered pipeline behaviors from DI.
/// </summary>
internal sealed class QueryBus : IQueryBus
{
    private readonly IServiceProvider _serviceProvider;

    public QueryBus(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public Task<TResponse> SendAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var queryType = query.GetType();
        var method = typeof(QueryBus)
            .GetMethod(nameof(SendCoreAsync), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .MakeGenericMethod(queryType, typeof(TResponse));

        return (Task<TResponse>)method.Invoke(this, [query, cancellationToken])!;
    }

    private Task<TResponse> SendCoreAsync<TQuery, TResponse>(TQuery query, CancellationToken cancellationToken)
        where TQuery : IQuery<TResponse>
    {
        var handler = _serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResponse>>();
        RequestHandlerDelegate<TResponse> next = () => handler.HandleAsync(query, cancellationToken);

        var behaviors = _serviceProvider
            .GetServices<IPipelineBehavior<TQuery, TResponse>>()
            .OrderByDescending(b => b.Order);

        foreach (var behavior in behaviors)
        {
            var captured = behavior;
            var current = next;
            next = () => captured.Handle(query, current, cancellationToken);
        }

        return next();
    }
}
