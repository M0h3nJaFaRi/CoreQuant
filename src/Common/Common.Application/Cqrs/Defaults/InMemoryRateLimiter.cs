using Common.Application.RateLimiting;

namespace Common.Application.Cqrs.Defaults;

/// <summary>
/// Process-local fixed-window rate limiter for single-node / test scenarios.
/// Production multi-pod deployments should replace this with a distributed implementation.
/// </summary>
internal sealed class InMemoryRateLimiter : IRateLimiter
{
    private readonly object _sync = new();
    private readonly Dictionary<string, WindowCounter> _windows = new(StringComparer.Ordinal);

    /// <inheritdoc />
    public Task<RateLimitAcquireResult> TryAcquireAsync(
        string partitionKey,
        int permitLimit,
        TimeSpan window,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(partitionKey);
        cancellationToken.ThrowIfCancellationRequested();

        if (permitLimit <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(permitLimit), permitLimit, "Permit limit must be greater than zero.");
        }

        if (window <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(window), window, "Window must be greater than zero.");
        }

        lock (_sync)
        {
            var now = DateTimeOffset.UtcNow;
            if (!_windows.TryGetValue(partitionKey, out var counter) || now >= counter.WindowEndUtc)
            {
                counter = new WindowCounter(now, now.Add(window), 0);
                _windows[partitionKey] = counter;
            }

            if (counter.Count >= permitLimit)
            {
                var retryAfter = counter.WindowEndUtc - now;
                return Task.FromResult(new RateLimitAcquireResult(
                    false,
                    retryAfter > TimeSpan.Zero ? retryAfter : window));
            }

            counter.Count++;
            return Task.FromResult(new RateLimitAcquireResult(true, null));
        }
    }

    private sealed class WindowCounter(DateTimeOffset windowStartUtc, DateTimeOffset windowEndUtc, int count)
    {
        public DateTimeOffset WindowStartUtc { get; } = windowStartUtc;

        public DateTimeOffset WindowEndUtc { get; } = windowEndUtc;

        public int Count { get; set; } = count;
    }
}
