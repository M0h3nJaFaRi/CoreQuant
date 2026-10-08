using Framework.Time;
using StackExchange.Redis;

namespace Framework.Messaging.Lease;

/// <summary>
/// Redis-backed <see cref="IWorkLease"/> using SET NX + PX.
/// </summary>
internal sealed class RedisWorkLease(
    IConnectionMultiplexer multiplexer,
    ISystemClock clock,
    string keyPrefix = "framework:lease:") : IWorkLease
{
    private readonly string _keyPrefix = keyPrefix.EndsWith(':') ? keyPrefix : keyPrefix + ":";

    /// <inheritdoc />
    public async Task<IWorkLeaseHandle?> TryAcquireAsync(
        string resourceKey,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceKey);
        if (leaseDuration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(leaseDuration), leaseDuration, "Lease duration must be positive.");

        cancellationToken.ThrowIfCancellationRequested();

        var token = Guid.NewGuid().ToString("N");
        var redisKey = _keyPrefix + resourceKey;
        var db = multiplexer.GetDatabase();
        var acquired = await db.StringSetAsync(
                redisKey,
                token,
                leaseDuration,
                When.NotExists)
            .ConfigureAwait(false);

        if (!acquired)
            return null;

        return new Handle(db, redisKey, resourceKey, token, clock.UtcNow.Add(leaseDuration));
    }

    private sealed class Handle(
        IDatabase database,
        string redisKey,
        string resourceKey,
        string token,
        DateTimeOffset expiresAtUtc) : IWorkLeaseHandle
    {
        private static readonly LuaScript ReleaseScript = LuaScript.Prepare(
            """
            if redis.call('get', @key) == @token then
              return redis.call('del', @key)
            else
              return 0
            end
            """);

        private int _disposed;

        public string ResourceKey { get; } = resourceKey;

        public DateTimeOffset ExpiresAtUtc { get; } = expiresAtUtc;

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            await database.ScriptEvaluateAsync(
                    ReleaseScript,
                    new { key = (RedisKey)redisKey, token = (RedisValue)token })
                .ConfigureAwait(false);
        }
    }
}
