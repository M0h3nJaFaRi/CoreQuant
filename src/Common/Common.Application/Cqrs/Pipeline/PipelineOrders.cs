namespace Common.Application.Cqrs.Pipeline;

/// <summary>
/// Deterministic pipeline order constants (outer → inner).
/// </summary>
internal static class PipelineOrders
{
    /// <summary>Outermost: request lifecycle logging.</summary>
    public const int Logging = 100;

    /// <summary>CQRS metrics.</summary>
    public const int Metrics = 200;

    /// <summary>Duration measurement and threshold warnings.</summary>
    public const int Performance = 300;

    /// <summary>FluentValidation.</summary>
    public const int Validation = 400;

    /// <summary>Use-case authorization.</summary>
    public const int Authorization = 500;

    /// <summary>Command idempotency (opt-in).</summary>
    public const int Idempotency = 600;

    /// <summary>Query caching (opt-in).</summary>
    public const int Caching = 600;

    /// <summary>
    /// Per-partition rate limiting (after authorization; before idempotency/caching).
    /// </summary>
    public const int RateLimit = 550;
}
