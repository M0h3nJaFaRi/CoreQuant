using CoreQuant.Infrastructure.Redis.Core.Contract;
using CoreQuant.Infrastructure.Redis.Core.Scripts;

namespace CoreQuant.Infrastructure.Redis.Observability;

/// <summary>
/// Stable script identifiers for Redis metrics tags.
/// </summary>
internal static class RedisScriptNames
{
    public static string Resolve(RedisScript script)
    {
        if (ReferenceEquals(script, RedisScripts.SlidingWindowRateLimit)) return "sliding_window_rate_limit";

        if (ReferenceEquals(script, RedisScripts.GetDelString)) return "get_del";

        if (ReferenceEquals(script, RedisScripts.ReleaseLock)) return "release_lock";

        return "unknown";
    }
}