namespace Common.Application.Cqrs.Pipeline;

/// <summary>
/// Deterministic pipeline order constants (outer → inner).
/// </summary>
internal static class PipelineOrders
{
    /// <summary>Outermost: request lifecycle logging.</summary>
    public const int Logging = 200;

    /// <summary>CQRS metrics.</summary>
    public const int Metrics = 300;

    /// <summary>FluentValidation.</summary>
    public const int Validation = 500;

    /// <summary>Use-case authorization.</summary>
    public const int Authorization = 600;

    /// <summary>
    /// Per-partition rate limiting (after authorization; before idempotency/caching).
    /// </summary>
    public const int RateLimit = 700;

    /// <summary>Command idempotency (opt-in).</summary>
    public const int Idempotency = 800;

    /// <summary>Query caching (opt-in).</summary>
    public const int Caching = 900;
}