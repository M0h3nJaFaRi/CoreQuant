using CoreQuant.Infrastructure.Redis.Core.Contract;

namespace CoreQuant.Infrastructure.Redis.Core;

/// <summary>
/// Internal Redis access. Applies the CoreQuant host-role prefix and handles serialization.
/// </summary>
internal interface IRedisCore
{
    /// <summary>
    /// Gets a serialized value stored under <paramref name="key"/>.
    /// </summary>
    Task<(bool Found, T? Value)> GetAsync<T>(uint dbIndex, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores a serialized value under <paramref name="key"/> with expiry.
    /// </summary>
    Task SetAsync<T>(uint dbIndex, string key, T value, TimeSpan expiry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes <paramref name="key"/> when present.
    /// </summary>
    Task<bool> DeleteAsync(uint dbIndex, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a prepared Lua script against the selected database.
    /// Parameters named <c>key</c> (case-insensitive) receive the host-role prefix automatically.
    /// </summary>
    Task<RedisScriptResult> ExecuteScriptAsync(uint dbIndex, RedisScript script, IReadOnlyDictionary<string, object?> parameters, CancellationToken cancellationToken = default);
}