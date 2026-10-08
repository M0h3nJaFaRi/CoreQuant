namespace Common.Application.Cqrs.Abstractions;

/// <summary>
/// Opt-in marker for queries that may be served from cache.
/// </summary>
public interface ICacheableQuery<TResponse> : IQuery<TResponse>
{
    /// <summary>
    /// Cache key for this query.
    /// </summary>
    string CacheKey { get; }

    /// <summary>
    /// Absolute cache duration. Null uses the default configured duration.
    /// </summary>
    TimeSpan? CacheDuration { get; }
}
