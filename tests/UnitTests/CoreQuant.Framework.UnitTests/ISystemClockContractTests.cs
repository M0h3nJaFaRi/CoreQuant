using Framework.Time;

namespace CoreQuant.Framework.UnitTests;

public sealed class ISystemClockContractTests
{
    [Fact]
    public void ISystemClock_Exposes_UtcNow()
    {
        ISystemClock clock = new FixedClock(new DateTimeOffset(2026, 9, 19, 12, 0, 0, TimeSpan.Zero));

        Assert.Equal(new DateTimeOffset(2026, 9, 19, 12, 0, 0, TimeSpan.Zero), clock.UtcNow);
    }

    private sealed class FixedClock(DateTimeOffset utcNow) : ISystemClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
