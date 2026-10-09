using CoreQuant.Infrastructure.Redis.Configuration.Options;

namespace CoreQuant.Infrastructure.Redis.Observability;

/// <summary>
/// Maps a Redis logical database index to a stable feature label for metrics.
/// </summary>
internal static class RedisFeatureResolver
{
    /// <summary>
    /// Resolves the feature name for <paramref name="dbIndex"/> using configured database assignments.
    /// </summary>
    public static string Resolve(CoreQuantRedisOptions options, uint dbIndex)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (dbIndex == options.Cache.Database) return "cache";

        if (dbIndex == options.Idempotency.Database) return "idempotency";

        if (dbIndex == options.RateLimit.Database) return "rate_limit";

        if (dbIndex == options.Token.Database) return "token";

        if (dbIndex == options.Lock.Database) return "lock";

        if (dbIndex == options.EventStore.Database) return "event_store";

        if (dbIndex == options.Queue.Database) return "queue";

        return "unknown";
    }
}