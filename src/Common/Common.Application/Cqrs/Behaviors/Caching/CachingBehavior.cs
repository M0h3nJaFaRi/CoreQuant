using Common.Application.Caching;
using Common.Application.Configuration.Options;
using Common.Application.Cqrs.Abstractions.Queries;
using Common.Application.Cqrs.Pipeline;
using Common.Application.Cqrs.Pipeline.Context;
using Microsoft.Extensions.Options;

namespace Common.Application.Cqrs.Behaviors.Caching;

/// <summary>
/// Serves cacheable queries from <see cref="IRequestCache"/> when present.
/// </summary>
internal sealed class CachingBehavior<TRequest, TResponse>(IRequestCache cache,
                                                           IOptions<CachingBehaviorOptions> options, 
                                                           IRequestExecutionContext executionContext) : IPipelineBehavior<TRequest, TResponse>
{
    /// <inheritdoc />
    public int Order => PipelineOrders.Caching;

    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ICacheableQuery<TResponse> cacheable)
            return await next().ConfigureAwait(false);

        var key = cacheable.CacheKey;
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var (hit, cached) = await cache.TryGetAsync<TResponse>(key, cancellationToken).ConfigureAwait(false);

        if (hit)
        {
            executionContext.MarkServedFrom("cache");
            return cached!;
        }

        var response = await next().ConfigureAwait(false);
        var duration = cacheable.CacheDuration ?? options.Value.DefaultDuration;
        await cache.SetAsync(key, response, duration, cancellationToken).ConfigureAwait(false);

        return response;
    }
}