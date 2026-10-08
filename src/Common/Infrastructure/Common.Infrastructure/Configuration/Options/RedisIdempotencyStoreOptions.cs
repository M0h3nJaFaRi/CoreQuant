namespace Common.Infrastructure.Configuration.Options;

/// <summary>
/// Options for Redis-backed idempotency store.
/// </summary>
internal sealed class RedisIdempotencyStoreOptions
{
    public const string SectionName = "Infrastructure:IdempotencyStore";

    /// <summary>
    /// Redis key prefix for distributed idempotency entries.
    /// </summary>
    public string KeyPrefix { get; set; } = "cqrs:idempotency:";
}
