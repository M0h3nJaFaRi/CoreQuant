using System.Diagnostics;
using CoreQuant.Infrastructure.Redis.Configuration.Options;
using CoreQuant.Infrastructure.Redis.Core.Contract;
using CoreQuant.Infrastructure.Redis.Observability;
using Microsoft.Extensions.Options;

namespace CoreQuant.Infrastructure.Redis.Core;

/// <summary>
/// Records Redis operation metrics around <see cref="RedisCore"/>.
/// </summary>
internal sealed class RedisInstrumentedCore(RedisCore inner, RedisMetrics metrics, IOptionsMonitor<CoreQuantRedisOptions> options) : IRedisCore
{
    /// <inheritdoc />
    public Task SetAsync<T>(uint dbIndex, string key, T value, TimeSpan expiry, CancellationToken cancellationToken = default)
        => ExecuteAsync("set", dbIndex, () => inner.SetAsync(dbIndex, key, value, expiry, cancellationToken));

    /// <inheritdoc />
    public Task<bool> SetIfNotExistsAsync<T>(uint dbIndex, string key, T value, TimeSpan expiry, CancellationToken cancellationToken = default)
        => ExecuteAsync("set_nx", dbIndex, () => inner.SetIfNotExistsAsync(dbIndex, key, value, expiry, cancellationToken));

    /// <inheritdoc />
    public Task<(bool Found, T? Value)> GetAsync<T>(uint dbIndex, string key, CancellationToken cancellationToken = default)
        => ExecuteAsync("get", dbIndex, () => inner.GetAsync<T>(dbIndex, key, cancellationToken));

    /// <inheritdoc />
    public Task<(bool Removed, T? Value)> TryRemoveAsync<T>(uint dbIndex, string key, CancellationToken cancellationToken = default)
        => ExecuteAsync("remove", dbIndex, () => inner.TryRemoveAsync<T>(dbIndex, key, cancellationToken));

    /// <inheritdoc />
    public Task<bool> RemoveAsync(uint dbIndex, string key, CancellationToken cancellationToken = default)
        => ExecuteAsync("delete", dbIndex, () => inner.RemoveAsync(dbIndex, key, cancellationToken));

    /// <inheritdoc />
    public Task<RedisScriptResult> ExecuteScriptAsync(uint dbIndex, RedisScript script, IReadOnlyDictionary<string, object?> parameters, CancellationToken cancellationToken = default)
    {
        var scriptName = RedisScriptNames.Resolve(script);
        return ExecuteAsync("script", dbIndex, () => inner.ExecuteScriptAsync(dbIndex, script, parameters, cancellationToken), scriptName);
    }

    /// <inheritdoc />
    public Task<string> StreamAddAsync<T>(uint dbIndex, string streamKey, T payload, int maxLength, CancellationToken cancellationToken = default)
        => ExecuteAsync("stream_add", dbIndex, () => inner.StreamAddAsync(dbIndex, streamKey, payload, maxLength, cancellationToken));

    /// <inheritdoc />
    public Task EnsureConsumerGroupAsync(uint dbIndex, string streamKey, string consumerGroup, CancellationToken cancellationToken = default)
        => ExecuteAsync("ensure_group", dbIndex, () => inner.EnsureConsumerGroupAsync(dbIndex, streamKey, consumerGroup, cancellationToken));

    /// <inheritdoc />
    public Task<IReadOnlyList<RedisStreamEntry<T>>> StreamReadGroupAsync<T>(uint dbIndex, string streamKey, string consumerGroup, string consumerName, int count, CancellationToken cancellationToken = default)
        => ExecuteAsync("stream_read", dbIndex, () => inner.StreamReadGroupAsync<T>(dbIndex, streamKey, consumerGroup, consumerName, count, cancellationToken));

    /// <inheritdoc />
    public Task StreamAckAsync(uint dbIndex, string streamKey, string consumerGroup, string entryId, CancellationToken cancellationToken = default)
        => ExecuteAsync("stream_ack", dbIndex, () => inner.StreamAckAsync(dbIndex, streamKey, consumerGroup, entryId, cancellationToken));

    /// <inheritdoc />
    public Task PingAsync(uint dbIndex, CancellationToken cancellationToken = default)
        => ExecuteAsync("ping", dbIndex, () => inner.PingAsync(dbIndex, cancellationToken));

    private async Task ExecuteAsync(string operation, uint dbIndex, Func<Task> action, string? script = null)
    {
        var feature = RedisFeatureResolver.Resolve(options.CurrentValue, dbIndex);
        var started = Stopwatch.GetTimestamp();
        try
        {
            await action().ConfigureAwait(false);
            RecordSuccess(operation, dbIndex, feature, started, script);
        }
        catch
        {
            RecordFailure(operation, dbIndex, feature, started, script);
            throw;
        }
    }

    private async Task<T> ExecuteAsync<T>(string operation, uint dbIndex, Func<Task<T>> action, string? script = null)
    {
        var feature = RedisFeatureResolver.Resolve(options.CurrentValue, dbIndex);
        var started = Stopwatch.GetTimestamp();
        try
        {
            var result = await action().ConfigureAwait(false);
            RecordSuccess(operation, dbIndex, feature, started, script);
            return result;
        }
        catch
        {
            RecordFailure(operation, dbIndex, feature, started, script);
            throw;
        }
    }

    private void RecordSuccess(string operation, uint dbIndex, string feature, long started, string? script)
    {
        var durationMs = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
        metrics.RecordSuccess(operation, dbIndex, feature, durationMs, script);
    }

    private void RecordFailure(string operation, uint dbIndex, string feature, long started, string? script)
    {
        var durationMs = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
        metrics.RecordFailure(operation, dbIndex, feature, durationMs, script);
    }
}
