using Framework.Messaging.Lease;
using Framework.Time;
using CoreQuant.Infrastructure.Redis.Configuration.Options;
using CoreQuant.Infrastructure.Redis.KeyPrefix;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace CoreQuant.Infrastructure.Redis.Messaging;

/// <summary>
/// Redis-backed <see cref="IWorkLease"/> using SET NX + PX.
/// </summary>
internal sealed class RedisWorkLease : IWorkLease
{
    private readonly IConnectionMultiplexer _multiplexer;
    private readonly ISystemClock _clock;
    private readonly string _keyPrefix;

    public RedisWorkLease(
        IConnectionMultiplexer multiplexer,
        ISystemClock clock,
        IOptions<RedisKeyNamespaceOptions> namespaceOptions)
    {
        ArgumentNullException.ThrowIfNull(multiplexer);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(namespaceOptions);

        _multiplexer = multiplexer;
        _clock = clock;
        _keyPrefix = RedisKeyPrefixBuilder.BuildFeaturePrefix(
            namespaceOptions.Value,
            RedisKeyPrefixBuilder.LeaseFeature);
    }

    /// <inheritdoc />
    public async Task<IWorkLeaseHandle?> TryAcquireAsync(
        string resourceKey,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceKey);
        if (leaseDuration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(leaseDuration), leaseDuration, "Lease duration must be positive.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var token = Guid.NewGuid().ToString("N");
        var redisKey = _keyPrefix + resourceKey;
        var db = _multiplexer.GetDatabase();
        var acquired = await db.StringSetAsync(
                redisKey,
                token,
                leaseDuration,
                When.NotExists)
            .ConfigureAwait(false);

        if (!acquired)
        {
            return null;
        }

        return new Handle(db, redisKey, resourceKey, token, _clock.UtcNow.Add(leaseDuration));
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
            {
                return;
            }

            await database.ScriptEvaluateAsync(
                    ReleaseScript,
                    new { key = (RedisKey)redisKey, token = (RedisValue)token })
                .ConfigureAwait(false);
        }
    }
}
