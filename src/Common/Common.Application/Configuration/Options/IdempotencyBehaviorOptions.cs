namespace Common.Application.Configuration.Options;

/// <summary>
/// Options for CQRS idempotency behavior.
/// </summary>
internal sealed class IdempotencyBehaviorOptions
{
    public const string SectionName = "Cqrs:Idempotency";

    /// <summary>
    /// Default TTL for stored idempotent responses.
    /// </summary>
    public TimeSpan DefaultTtl { get; set; } = TimeSpan.FromHours(24);
}
