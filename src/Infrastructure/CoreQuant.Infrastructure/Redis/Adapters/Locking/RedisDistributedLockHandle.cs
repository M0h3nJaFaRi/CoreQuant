using Common.Application.Locking;
using CoreQuant.Infrastructure.Redis.Core;
using CoreQuant.Infrastructure.Redis.Core.Scripts;

namespace CoreQuant.Infrastructure.Redis.Adapters.Locking;

/// <summary>
/// Handle for an acquired Redis lock.
/// </summary>
internal sealed class RedisDistributedLockHandle(IRedisCore redis, uint database, string resource, string ownerToken) : IRedisDistributedLockHandle
{
    private int _released;

    /// <inheritdoc />
    public string Resource { get; } = resource;

    /// <inheritdoc />
    public async Task ReleaseAsync(CancellationToken cancellationToken = default)
    {
        if (Interlocked.Exchange(ref _released, 1) == 1) return;

        var parameters = new Dictionary<string, object?>
        {
            ["key"] = BuildKey(Resource),
            ["token"] = ownerToken
        };

        await redis.ExecuteScriptAsync(database, RedisScripts.ReleaseLock, parameters, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync() => new(ReleaseAsync());

    internal static string BuildKey(string resource) => $"lock:{resource.Trim()}";
}