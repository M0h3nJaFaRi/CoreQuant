using StackExchange.Redis;

namespace CoreQuant.Infrastructure.Redis.Core.Contract;

/// <summary>
/// Wraps a prepared Redis Lua script for use through <see cref="IRedisCore"/>.
/// </summary>
internal sealed class RedisScript
{
    private RedisScript(LuaScript inner)
    {
        Inner = inner;
    }

    /// <summary>
    /// Underlying StackExchange.Redis script (Core-only).
    /// </summary>
    internal LuaScript Inner { get; }

    /// <summary>
    /// Prepares a script for repeated execution.
    /// </summary>
    public static RedisScript Prepare(string script)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(script);
        return new RedisScript(LuaScript.Prepare(script));
    }
}
