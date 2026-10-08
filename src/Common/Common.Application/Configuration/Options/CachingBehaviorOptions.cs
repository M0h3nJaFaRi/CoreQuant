namespace Common.Application.Configuration.Options;

/// <summary>
/// Options for CQRS query caching behavior.
/// </summary>
internal sealed class CachingBehaviorOptions
{
    public const string SectionName = "Cqrs:Caching";

    /// <summary>
    /// Default cache duration when a query does not specify one.
    /// </summary>
    public TimeSpan DefaultDuration { get; set; } = TimeSpan.FromMinutes(1);
}
