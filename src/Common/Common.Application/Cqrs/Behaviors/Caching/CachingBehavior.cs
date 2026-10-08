using Common.Application.Caching;
using Common.Application.Configuration.Options;
using Common.Application.Cqrs.Abstractions;
using Common.Application.Cqrs.Pipeline;
using Microsoft.Extensions.Options;

namespace Common.Application.Cqrs.Behaviors.Caching;

/// <summary>
/// Serves cacheable queries from <see cref="IRequestCache"/> when present.
/// </summary>
internal sealed class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IRequestCache _cache;
    private readonly CachingBehaviorOptions _options;

    public CachingBehavior(IRequestCache cache, IOptions<CachingBehaviorOptions> options)
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(options);
        _cache = cache;
        _options = options.Value;
    }

    /// <inheritdoc />
    public int Order => PipelineOrders.Caching;

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICacheableQuery<TResponse> cacheable)
        {
            return await next().ConfigureAwait(false);
        }

        var key = cacheable.CacheKey;
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var (hit, cached) = await _cache.TryGetAsync<TResponse>(key, cancellationToken).ConfigureAwait(false);
        if (hit)
        {
            return cached!;
        }

        var response = await next().ConfigureAwait(false);
        var duration = cacheable.CacheDuration ?? _options.DefaultDuration;
        await _cache.SetAsync(key, response, duration, cancellationToken).ConfigureAwait(false);
        return response;
    }
}
