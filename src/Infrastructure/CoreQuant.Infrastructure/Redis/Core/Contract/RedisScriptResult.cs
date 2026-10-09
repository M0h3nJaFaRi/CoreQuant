using StackExchange.Redis;

namespace CoreQuant.Infrastructure.Redis.Core.Contract;

/// <summary>
/// Wrapped Redis script evaluation result.
/// </summary>
internal sealed class RedisScriptResult
{
    private readonly RedisResult _result;

    private RedisScriptResult(RedisResult result)
    {
        _result = result;
    }

    /// <summary>
    /// Whether the script returned null.
    /// </summary>
    public bool IsNull => _result.IsNull;

    /// <summary>
    /// Creates a wrapper around a provider result.
    /// </summary>
    public static RedisScriptResult From(RedisResult result) => new(result);

    /// <summary>
    /// Reads the result as a string when present.
    /// </summary>
    public string? AsString() => _result.IsNull ? null : (string)_result!;

    /// <summary>
    /// Reads the result as <see cref="int"/>.
    /// </summary>
    public int AsInt32() => (int)_result;

    /// <summary>
    /// Reads the result as <see cref="long"/>.
    /// </summary>
    public long AsInt64() => (long)_result;

    /// <summary>
    /// Reads the result as an array of nested script results.
    /// </summary>
    public IReadOnlyList<RedisScriptResult> AsArray()
    {
        if (_result.IsNull)
        {
            return [];
        }

        var items = (RedisResult[])_result!;
        var results = new RedisScriptResult[items.Length];
        for (var i = 0; i < items.Length; i++)
        {
            results[i] = From(items[i]);
        }

        return results;
    }
}
