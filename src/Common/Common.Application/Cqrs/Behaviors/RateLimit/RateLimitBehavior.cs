using Microsoft.Extensions.Options;
using Common.Application.Exceptions;
using Common.Application.RateLimiting;
using Common.Application.Cqrs.Pipeline;
using Common.Application.Cqrs.Abstractions;
using Common.Application.Configuration.Options;
using Common.Application.Authorization.Abstraction;

namespace Common.Application.Cqrs.Behaviors.RateLimit;

/// <summary>
/// Enforces per-partition rate limits before the handler runs.
/// </summary>
internal sealed class RateLimitBehavior<TRequest, TResponse>(IRedisRateLimiter rateLimiter,
                                                             ICurrentUserAccessor currentUser,
                                                             IOptions<RateLimitBehaviorOptions> options) : IPipelineBehavior<TRequest, TResponse>
{
    /// <inheritdoc />
    public int Order => PipelineOrders.RateLimit;

    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled)
            return await next().ConfigureAwait(false);

        var window = ResolveWindow(request);
        var requestType = typeof(TRequest).Name;
        var permitLimit = ResolvePermitLimit(request);
        var partitionKey = BuildPartitionKey(request, requestType);

        var acquire = await rateLimiter.TryAcquireAsync(partitionKey, permitLimit, window, cancellationToken).ConfigureAwait(false);

        if (!acquire.Acquired)
            throw new RateLimitExceededException($"Rate limit exceeded for '{requestType}'.", acquire.RetryAfter);

        return await next().ConfigureAwait(false);
    }

    private TimeSpan ResolveWindow(TRequest request)
    {
        if (request is IRateLimitedRequest rateLimited && rateLimited.Window is { } window && window > TimeSpan.Zero)
            return window;

        return options.Value.DefaultWindow;
    }
    private uint ResolvePermitLimit(TRequest request)
    {
        if (request is IRateLimitedRequest rateLimited && rateLimited.PermitLimit.HasValue && rateLimited.PermitLimit.Value > 0)
            return (uint)rateLimited.PermitLimit.Value;

        return (uint)options.Value.DefaultPermitLimit;
    }
    private string BuildPartitionKey(TRequest request, string requestType)
    {
        if (request is IRateLimitedRequest rateLimited && !string.IsNullOrWhiteSpace(rateLimited.RateLimitKey))
            return rateLimited.RateLimitKey;

        var userId = currentUser.UserId?.ToString() ?? "anon";

        return $"{userId}:{requestType}";
    }
}