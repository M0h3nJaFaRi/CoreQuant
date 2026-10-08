namespace Framework.Time;

/// <summary>
/// Abstraction over the system clock for UTC time. Implementations live in Framework.Infrastructure.
/// </summary>
public interface ISystemClock
{
    /// <summary>
    /// Gets the current UTC time as a <see cref="DateTimeOffset"/>.
    /// </summary>
    DateTimeOffset UtcNow { get; }
}
