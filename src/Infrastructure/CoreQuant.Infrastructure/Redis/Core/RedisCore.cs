using System.Dynamic;
using CoreQuant.Infrastructure.Redis.Configuration.Options;
using CoreQuant.Infrastructure.Redis.Core.Contract;
using CoreQuant.Infrastructure.Redis.Core.Scripts;
using Framework.Serialization.Json;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace CoreQuant.Infrastructure.Redis.Core;

/// <summary>
/// Default Redis core that owns the connection multiplexer and key prefixing.
/// </summary>
internal sealed class RedisCore(IOptionsMonitor<CoreQuantRedisOptions> options, IJsonSerializer serializer) : IRedisCore, IAsyncDisposable
{
    private const string StreamPayloadField = "json";
    private readonly string _keyPrefix = $"CoreQuant:{options.CurrentValue.HostRole.Trim()}";
    private readonly IConnectionMultiplexer _multiplexer = ConnectionMultiplexer.Connect(options.CurrentValue.ToConfigurationOptions());

    /// <inheritdoc />
    public async Task<(bool Found, T? Value)> GetAsync<T>(uint dbIndex, string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();

        var value = await GetDatabase(dbIndex).StringGetAsync(ApplyPrefix(key)).ConfigureAwait(false);

        if (value.IsNullOrEmpty)
        {
            return (false, default);
        }

        return (true, DeserializeValue<T>((string)value!));
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(uint dbIndex, string key, T value, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        if (expiry <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(expiry), expiry, "Expiry must be positive.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var payload = SerializeValue(value);
        await GetDatabase(dbIndex)
            .StringSetAsync(ApplyPrefix(key), payload, expiry)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> SetIfNotExistsAsync<T>(uint dbIndex, string key, T value, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        if (expiry <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(expiry), expiry, "Expiry must be positive.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var payload = SerializeValue(value);
        return await GetDatabase(dbIndex)
            .StringSetAsync(ApplyPrefix(key), payload, expiry, When.NotExists)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<(bool Removed, T? Value)> TryRemoveAsync<T>(uint dbIndex, string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await ExecuteScriptAsync(
                dbIndex,
                RedisScripts.GetDelString,
                new Dictionary<string, object?> { ["key"] = key },
                cancellationToken)
            .ConfigureAwait(false);

        if (result.IsNull)
        {
            return (false, default);
        }

        var raw = result.AsString();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return (false, default);
        }

        return (true, DeserializeValue<T>(raw));
    }

    /// <inheritdoc />
    public async Task<bool> RemoveAsync(uint dbIndex, string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();

        return await GetDatabase(dbIndex)
            .KeyDeleteAsync(ApplyPrefix(key))
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<RedisScriptResult> ExecuteScriptAsync(uint dbIndex, RedisScript script, IReadOnlyDictionary<string, object?> parameters, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(script);
        ArgumentNullException.ThrowIfNull(parameters);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await GetDatabase(dbIndex)
            .ScriptEvaluateAsync(script.Inner, ToScriptParameters(parameters))
            .ConfigureAwait(false);

        return RedisScriptResult.From(result);
    }

    /// <inheritdoc />
    public async Task<string> StreamAddAsync<T>(uint dbIndex, string streamKey, T payload, int maxLength, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var entries = new[] { new NameValueEntry(StreamPayloadField, serializer.Serialize(payload)) };
        var prefixedStream = ApplyPrefix(streamKey);
        var db = GetDatabase(dbIndex);

        return (maxLength > 0) ?
               (await db.StreamAddAsync(prefixedStream, entries, maxLength: maxLength, useApproximateMaxLength: true).ConfigureAwait(false)).ToString() :
               (await db.StreamAddAsync(prefixedStream, entries).ConfigureAwait(false)).ToString();
    }

    /// <inheritdoc />
    public async Task EnsureConsumerGroupAsync(uint dbIndex, string streamKey, string consumerGroup, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(streamKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(consumerGroup);
        cancellationToken.ThrowIfCancellationRequested();

        var db = GetDatabase(dbIndex);
        var prefixedStream = ApplyPrefix(streamKey);

        try
        {
            await db.StreamCreateConsumerGroupAsync(prefixedStream, consumerGroup, StreamPosition.NewMessages, createStream: true).ConfigureAwait(false);
        }
        catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP", StringComparison.OrdinalIgnoreCase))
        {
            // Group already exists.
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RedisStreamEntry<T>>> StreamReadGroupAsync<T>(uint dbIndex, string streamKey, string consumerGroup, string consumerName, int count, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(streamKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(consumerGroup);
        ArgumentException.ThrowIfNullOrWhiteSpace(consumerName);
        
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count), count, "Count must be greater than zero.");

        cancellationToken.ThrowIfCancellationRequested();

        var entries = await GetDatabase(dbIndex).StreamReadGroupAsync(ApplyPrefix(streamKey), consumerGroup, consumerName, position: StreamPosition.NewMessages, count: count)
                                                .ConfigureAwait(false);

        if (entries.Length == 0) return [];

        var records = new List<RedisStreamEntry<T>>(entries.Length);
        foreach (var entry in entries)
        {
            var json = entry.Values.FirstOrDefault(value => value.Name == StreamPayloadField).Value.ToString();
            if (string.IsNullOrWhiteSpace(json))
            {
                continue;
            }

            var payload = serializer.Deserialize<T>(json);
            if (payload is null)
            {
                continue;
            }

            records.Add(new RedisStreamEntry<T>
            {
                EntryId = entry.Id.ToString(),
                Payload = payload
            });
        }

        return records;
    }

    /// <inheritdoc />
    public async Task StreamAckAsync(uint dbIndex, string streamKey, string consumerGroup, string entryId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(streamKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(consumerGroup);
        ArgumentException.ThrowIfNullOrWhiteSpace(entryId);
        cancellationToken.ThrowIfCancellationRequested();

        await GetDatabase(dbIndex)
            .StreamAcknowledgeAsync(ApplyPrefix(streamKey), consumerGroup, entryId)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await _multiplexer.DisposeAsync().ConfigureAwait(false);
    }

    #region Private

    private string SerializeValue<T>(T value)
    {
        if (value is RedisLockOwner lockOwner)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(lockOwner.Token);
            return lockOwner.Token;
        }

        return serializer.Serialize(value);
    }

    private T? DeserializeValue<T>(string raw)
    {
        if (typeof(T) == typeof(RedisLockOwner))
        {
            return (T)(object)new RedisLockOwner(raw);
        }

        return serializer.Deserialize<T>(raw);
    }

    private string ApplyPrefix(string key) => $"{_keyPrefix}:{key.TrimStart(':').Trim()}";

    private IDatabase GetDatabase(uint dbIndex) => _multiplexer.GetDatabase((int)dbIndex);

    private object ToScriptParameters(IReadOnlyDictionary<string, object?> parameters)
    {
        IDictionary<string, object?> expando = new ExpandoObject();

        foreach (var (name, value) in parameters)
        {
            if (string.Equals(name, "key", StringComparison.OrdinalIgnoreCase) && value is string keyValue)
            {
                expando[name] = ApplyPrefix(keyValue);
                continue;
            }

            expando[name] = ConvertParameter(value);
        }

        return expando;
    }

    private static RedisValue ConvertParameter(object? value)
    {
        return value switch
        {
            null => RedisValue.Null,
            string s => s,
            int i => i,
            long l => l,
            double d => d,
            bool b => b,
            _ => RedisValue.Unbox(value)
        };
    }

    #endregion
}