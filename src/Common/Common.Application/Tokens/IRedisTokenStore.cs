namespace Common.Application.Tokens;

/// <summary>
/// Distributed token store backed by Redis (multi-pod safe).
/// </summary>
public interface IRedisTokenStore
{
    /// <summary>
    /// Stores a token under <paramref name="provider"/> with mandatory expiry.
    /// </summary>
    Task StoreAsync<T>(string provider, string key, T tokenModel, TimeSpan ttl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads a token without removing it.
    /// </summary>
    Task<(bool Found, T? Token)> TryGetAsync<T>(string provider, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically reads and deletes a one-time token.
    /// </summary>
    Task<(bool Consumed, T? TokenModel)> TryConsumeAsync<T>(string provider, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a token when present.
    /// </summary>
    Task<bool> RevokeAsync(string purpose, string tokenId, CancellationToken cancellationToken = default);
}