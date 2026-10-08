using Framework.Time;
using Microsoft.Extensions.DependencyInjection;

namespace Framework.Infrastructure.UnitTests;

public sealed class SystemClockTests
{
    [Fact]
    public void AddFrameworkInfrastructure_Registers_ISystemClock_As_Singleton()
    {
        var services = new ServiceCollection();
        services.AddFrameworkInfrastructure();

        using var provider = services.BuildServiceProvider();
        var clock1 = provider.GetRequiredService<ISystemClock>();
        var clock2 = provider.GetRequiredService<ISystemClock>();

        Assert.Same(clock1, clock2);
    }

    [Fact]
    public void SystemClock_UtcNow_Is_Close_To_DateTimeOffset_UtcNow()
    {
        var services = new ServiceCollection();
        services.AddFrameworkInfrastructure();
        using var provider = services.BuildServiceProvider();
        var clock = provider.GetRequiredService<ISystemClock>();

        var before = DateTimeOffset.UtcNow;
        var actual = clock.UtcNow;
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(actual, before.AddSeconds(-1), after.AddSeconds(1));
        Assert.Equal(TimeSpan.Zero, actual.Offset);
    }
}
