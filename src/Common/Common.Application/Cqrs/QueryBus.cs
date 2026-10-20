using Common.Application.Cqrs.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Common.Application.Cqrs.Abstractions.Queries;
using Common.Application.Cqrs.Abstractions.Queries.Bus;
using Common.Application.Cqrs.Abstractions.Queries.Handler;

namespace Common.Application.Cqrs;

/// <summary>
/// Default <see cref="IQueryBus"/> resolving handlers and ordered pipeline behaviors from DI.
/// </summary>
internal sealed class QueryBus(IServiceProvider serviceProvider) : IQueryBus
{
    /// <inheritdoc />
    public Task<TResponse> SendAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var queryType = query.GetType();

        var method = typeof(QueryBus).GetMethod(nameof(SendCoreAsync), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                                     .MakeGenericMethod(queryType, typeof(TResponse));

        return (Task<TResponse>)method.Invoke(this, [query, cancellationToken])!;
    }

    private Task<TResponse> SendCoreAsync<TQuery, TResponse>(TQuery query, CancellationToken cancellationToken)
    where TQuery : IQuery<TResponse>
    {
        var handler = serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResponse>>();
        
        RequestHandlerDelegate<TResponse> next = () => handler.HandleAsync(query, cancellationToken);

        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TQuery, TResponse>>()
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