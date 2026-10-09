using Common.Application.Locking;
using Microsoft.Extensions.Options;
using CoreQuant.Infrastructure.Redis.Core;
using CoreQuant.Infrastructure.Redis.Core.Contract;
using CoreQuant.Infrastructure.Redis.Configuration.Options;

namespace CoreQuant.Infrastructure.Redis.Adapters.Locking;

/// <summary>
/// Redis-backed implementation of <see cref="IRedisDistributedLock"/>.
/// </summary>
internal sealed class RedisDistributedLock(IRedisCore redis, IOptions<CoreQuantRedisOptions> options) : IRedisDistributedLock
{
    private readonly uint _database = options.Value.Lock.Database;

    /// <inheritdoc />
    public async Task<IRedisDistributedLockHandle?> TryAcquireAsync(string resource, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resource);

        if (ttl <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(ttl), ttl, "TTL must be positive.");

        var ownerToken = Guid.NewGuid().ToString("N");
        
        var lockOwner = new RedisLockOwner(ownerToken);

        var acquired = await redis.SetIfNotExistsAsync(_database, RedisDistributedLockHandle.BuildKey(resource), lockOwner, ttl, cancellationToken).ConfigureAwait(false);

        return acquired ? new RedisDistributedLockHandle(redis, _database, resource.Trim(), ownerToken) : null;
    }
}