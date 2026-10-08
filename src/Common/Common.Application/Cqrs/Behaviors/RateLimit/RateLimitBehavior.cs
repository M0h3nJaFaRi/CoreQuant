using Common.Application.Authorization;
using Common.Application.Configuration.Options;
using Common.Application.Cqrs.Abstractions;
using Common.Application.Cqrs.Pipeline;
using Common.Application.Exceptions;
using Common.Application.RateLimiting;
using Microsoft.Extensions.Options;

namespace Common.Application.Cqrs.Behaviors.RateLimit;

/// <summary>
/// Enforces per-partition rate limits before the handler runs.
/// </summary>
internal sealed class RateLimitBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IRedisRateLimiter _rateLimiter;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly RateLimitBehaviorOptions _options;

    /// <summary>
    /// Creates the behavior.
    /// </summary>
    public RateLimitBehavior(
        IRedisRateLimiter rateLimiter,
        ICurrentUserAccessor currentUser,
        IOptions<RateLimitBehaviorOptions> options)
    {
        ArgumentNullException.ThrowIfNull(rateLimiter);
        ArgumentNullException.ThrowIfNull(currentUser);
        ArgumentNullException.ThrowIfNull(options);
        _rateLimiter = rateLimiter;
        _currentUser = currentUser;
        _options = options.Value;
    }

    /// <inheritdoc />
    public int Order => PipelineOrders.RateLimit;

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            return await next().ConfigureAwait(false);
        }

        var requestType = typeof(TRequest).Name;
        var partitionKey = BuildPartitionKey(request, requestType);
        var permitLimit = ResolvePermitLimit(request);
        var window = ResolveWindow(request);

        var acquire = await _rateLimiter
            .TryAcquireAsync(partitionKey, permitLimit, window, cancellationToken)
            .ConfigureAwait(false);

        if (!acquire.Acquired)
        {
            throw new RateLimitExceededException(
                $"Rate limit exceeded for '{requestType}'.",
                acquire.RetryAfter);
        }

        return await next().ConfigureAwait(false);
    }

    private string BuildPartitionKey(TRequest request, string requestType)
    {
        if (request is IRateLimitedRequest rateLimited
            && !string.IsNullOrWhiteSpace(rateLimited.RateLimitKey))
        {
            return rateLimited.RateLimitKey;
        }

        var userId = _currentUser.UserId?.ToString() ?? "anon";
        return $"{userId}:{requestType}";
    }

    private uint ResolvePermitLimit(TRequest request)
    {
        if (request is IRateLimitedRequest rateLimited
            && rateLimited.PermitLimit.HasValue
            && rateLimited.PermitLimit.Value > 0)
        {
            return (uint)rateLimited.PermitLimit.Value;
        }

        return (uint)_options.DefaultPermitLimit;
    }

    private TimeSpan ResolveWindow(TRequest request)
    {
        if (request is IRateLimitedRequest rateLimited
            && rateLimited.Window is { } window
            && window > TimeSpan.Zero)
        {
            return window;
        }

        return _options.DefaultWindow;
    }
}
