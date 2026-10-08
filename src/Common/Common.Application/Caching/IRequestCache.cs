namespace Common.Application.Caching;

/// <summary>
/// Application cache abstraction for query responses.
/// </summary>
public interface IRequestCache
{
    /// <summary>
    /// Tries to get a cached value.
    /// </summary>
    Task<(bool Hit, T? Value)> TryGetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores a value with the given absolute duration.
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan duration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes <paramref name="key"/> when present.
    /// </summary>
    Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default);
}