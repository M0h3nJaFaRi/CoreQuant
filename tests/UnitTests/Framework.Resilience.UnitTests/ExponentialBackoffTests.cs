using Framework.Resilience.Backoff;

namespace Framework.Resilience.UnitTests;

public sealed class ExponentialBackoffTests
{
    [Fact]
    public void Calculate_Grows_Exponentially()
    {
        var baseDelay = TimeSpan.FromMilliseconds(100);

        Assert.Equal(TimeSpan.FromMilliseconds(100), ExponentialBackoff.Calculate(1, baseDelay));
        Assert.Equal(TimeSpan.FromMilliseconds(200), ExponentialBackoff.Calculate(2, baseDelay));
        Assert.Equal(TimeSpan.FromMilliseconds(400), ExponentialBackoff.Calculate(3, baseDelay));
    }

    [Fact]
    public void Calculate_Respects_MaxDelay()
    {
        var delay = ExponentialBackoff.Calculate(
            attempt: 10,
            baseDelay: TimeSpan.FromMilliseconds(100),
            maxDelay: TimeSpan.FromSeconds(1));

        Assert.Equal(TimeSpan.FromSeconds(1), delay);
    }

    [Fact]
    public void Calculate_Throws_For_Invalid_Attempt()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ExponentialBackoff.Calculate(0, TimeSpan.FromMilliseconds(10)));
    }
}
