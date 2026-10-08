namespace Framework.Time;

/// <summary>
/// Abstraction over the system clock for UTC time.
/// </summary>
public interface ISystemClock
{
    /// <summary>
    /// Gets the current UTC time as a <see cref="DateTimeOffset"/>.
    /// </summary>
    DateTimeOffset UtcNow { get; }
}
