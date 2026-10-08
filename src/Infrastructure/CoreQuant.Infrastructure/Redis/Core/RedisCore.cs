using System.Dynamic;
using CoreQuant.Infrastructure.Redis.Configuration.Options;
using CoreQuant.Infrastructure.Redis.Core.Contract;
using Framework.Serialization.Json;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace CoreQuant.Infrastructure.Redis.Core;

/// <summary>
/// Default Redis core that owns the connection multiplexer and key prefixing.
/// </summary>
internal sealed class RedisCore(IOptionsMonitor<CoreQuantRedisOptions> options, IJsonSerializer serializer) : IRedisCore, IAsyncDisposable
{
    private readonly string _keyPrefix = $"CoreQuant:{options.CurrentValue.HostRole.Trim()}";
    private readonly IConnectionMultiplexer _multiplexer = ConnectionMultiplexer.Connect(options.CurrentValue.ToConfigurationOptions());


    /// <inheritdoc />
    public async Task<(bool Found, T? Value)> GetAsync<T>(uint dbIndex, string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();

        var value = await GetDatabase(dbIndex).StringGetAsync(ApplyPrefix(key)).ConfigureAwait(false);

        if (value.IsNullOrEmpty) return (false, default);

        return (true, serializer.Deserialize<T>((string)value!));
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

        var payload = serializer.Serialize(value);
        await GetDatabase(dbIndex)
            .StringSetAsync(ApplyPrefix(key), payload, expiry)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(uint dbIndex, string key, CancellationToken cancellationToken = default)
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
    public async ValueTask DisposeAsync()
    {
        await _multiplexer.DisposeAsync().ConfigureAwait(false);
    }

    #region Private

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