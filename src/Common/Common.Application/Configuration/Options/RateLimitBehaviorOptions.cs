namespace Common.Application.Configuration.Options;

/// <summary>
/// Options for CQRS rate-limiting behavior.
/// </summary>
internal sealed class RateLimitBehaviorOptions
{
    public const string SectionName = "Cqrs:RateLimit";

    /// <summary>
    /// When false, the behavior passes through without enforcing limits.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Default maximum requests per window when a request does not override limits.
    /// </summary>
    public int DefaultPermitLimit { get; set; } = 100;

    /// <summary>
    /// Default sliding window duration when a request does not override it.
    /// </summary>
    public TimeSpan DefaultWindow { get; set; } = TimeSpan.FromMinutes(1);
}
