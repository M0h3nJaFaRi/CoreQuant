namespace Common.Infrastructure.Configuration.Options;

/// <summary>
/// Options for Redis-backed request cache.
/// </summary>
internal sealed class RedisRequestCacheOptions
{
    public const string SectionName = "Infrastructure:RequestCache";

    /// <summary>
    /// Redis key prefix for distributed query cache entries.
    /// </summary>
    public string KeyPrefix { get; set; } = "cqrs:cache:";
}
