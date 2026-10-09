using CoreQuant.Infrastructure.Redis.Core.Contract;

namespace CoreQuant.Infrastructure.Redis.Core;

/// <summary>
/// Internal Redis access. Applies the CoreQuant host-role prefix and handles serialization.
/// </summary>
internal interface IRedisCore
{
    /// <summary>
    /// Stores a serialized value under <paramref name="key"/> with expiry.
    /// </summary>
    Task SetAsync<T>(uint dbIndex, string key, T value, TimeSpan expiry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a value only when the key does not exist, with expiry.
    /// </summary>
    Task<bool> SetIfNotExistsAsync<T>(uint dbIndex, string key, T value, TimeSpan expiry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a serialized value stored under <paramref name="key"/>.
    /// </summary>
    Task<(bool Found, T? Value)> GetAsync<T>(uint dbIndex, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically removes a key and returns its deserialized value when present.
    /// </summary>
    Task<(bool Removed, T? Value)> TryRemoveAsync<T>(uint dbIndex, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes <paramref name="key"/> when present.
    /// </summary>
    Task<bool> RemoveAsync(uint dbIndex, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a prepared Lua script against the selected database.
    /// Parameters named <c>key</c> (case-insensitive) receive the host-role prefix automatically.
    /// </summary>
    Task<RedisScriptResult> ExecuteScriptAsync(uint dbIndex, RedisScript script, IReadOnlyDictionary<string, object?> parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Appends a serialized payload to a stream and returns the entry id.
    /// </summary>
    Task<string> StreamAddAsync<T>(uint dbIndex, string streamKey, T payload, int maxLength, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a consumer group when missing.
    /// </summary>
    Task EnsureConsumerGroupAsync(uint dbIndex, string streamKey, string consumerGroup, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads and deserializes new stream entries for a consumer group member.
    /// </summary>
    Task<IReadOnlyList<RedisStreamEntry<T>>> StreamReadGroupAsync<T>(uint dbIndex, string streamKey, string consumerGroup, string consumerName, int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// Acknowledges a stream entry for a consumer group.
    /// </summary>
    Task StreamAckAsync(uint dbIndex, string streamKey, string consumerGroup, string entryId, CancellationToken cancellationToken = default);
}