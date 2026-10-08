using Framework.Time;

namespace Framework.Infrastructure.Time;

/// <summary>
/// Default <see cref="ISystemClock"/> backed by <see cref="DateTimeOffset.UtcNow"/>.
/// </summary>
internal sealed class SystemClock : ISystemClock
{
    /// <inheritdoc />
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
