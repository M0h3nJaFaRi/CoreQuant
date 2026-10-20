namespace Framework.Configurations.Options;

/// <summary>
/// Shared health settings for any dependency (database, Redis, ...). Bound from a "Health" config block.
/// </summary>
public class DependencyHealthOptions
{
    public bool Enabled { get; set; }
    public bool Critical { get; set; }
    public TimeSpan Timeout { get; set; }
    public TimeSpan DegradedAfter { get; set; }
    public TimeSpan CacheDuration { get; set; }

    public void Validate(string name)
    {
        if (Timeout <= TimeSpan.Zero)
            throw new InvalidOperationException($"Health '{name}': Timeout must be > 0.");
        if (DegradedAfter <= TimeSpan.Zero || DegradedAfter >= Timeout)
            throw new InvalidOperationException($"Health '{name}': DegradedAfter must be > 0 and < Timeout.");
        if (CacheDuration < TimeSpan.Zero)
            throw new InvalidOperationException($"Health '{name}': CacheDuration must be >= 0.");
    }
}