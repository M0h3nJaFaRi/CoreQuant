using CoreQuant.Infrastructure.Redis.Configuration.Options;

namespace CoreQuant.Infrastructure.Redis.KeyPrefix;

/// <summary>
/// Builds Redis keys using the {Service}:{Host}:{feature} convention.
/// </summary>
internal static class RedisKeyPrefixBuilder
{
    /// <summary>
    /// Feature suffix for CQRS query cache entries.
    /// </summary>
    public const string CacheFeature = "cache";

    /// <summary>
    /// Feature suffix for CQRS idempotency entries.
    /// </summary>
    public const string IdempotencyFeature = "idempotency";

    /// <summary>
    /// Feature suffix for CQRS rate-limit windows.
    /// </summary>
    public const string RateLimitFeature = "ratelimit";

    /// <summary>
    /// Feature suffix for distributed work leases.
    /// </summary>
    public const string LeaseFeature = "lease";

    /// <summary>
    /// Builds the feature prefix ending with ':' ready for appending the partition key.
    /// </summary>
    public static string BuildFeaturePrefix(RedisKeyNamespaceOptions options, string featureSuffix)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(featureSuffix);

        return $"{options.Service.Trim()}:{options.Host.Trim()}:{featureSuffix.Trim()}:";
    }
}
