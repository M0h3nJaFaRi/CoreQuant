namespace Common.Application.Idempotency;

/// <summary>
/// Stores completed idempotent command results.
/// </summary>
public interface IIdempotencyStore
{
    /// <summary>
    /// Tries to get a previously stored response for <paramref name="key"/>.
    /// </summary>
    Task<(bool Found, TResponse? Response)> TryGetAsync<TResponse>(
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores a successful response for <paramref name="key"/>.
    /// </summary>
    Task SetAsync<TResponse>(
        string key,
        TResponse response,
        TimeSpan ttl,
        CancellationToken cancellationToken = default);
}
