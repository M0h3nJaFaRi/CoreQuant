using Framework.Infrastructure;
using Framework.Messaging.Lease;
using Microsoft.Extensions.DependencyInjection;

namespace Framework.Messaging.UnitTests;

public sealed class InMemoryWorkLeaseTests
{
    [Fact]
    public async Task TryAcquireAsync_Allows_Only_One_Active_Lease()
    {
        var services = new ServiceCollection();
        services.AddFrameworkInfrastructure();
        services.AddFrameworkMessaging();
        await using var provider = services.BuildServiceProvider();
        var lease = provider.GetRequiredService<IWorkLease>();

        await using var first = await lease.TryAcquireAsync("job-1", TimeSpan.FromSeconds(30));
        var second = await lease.TryAcquireAsync("job-1", TimeSpan.FromSeconds(30));

        Assert.NotNull(first);
        Assert.Null(second);
    }

    [Fact]
    public async Task DisposeAsync_Releases_Lease()
    {
        var services = new ServiceCollection();
        services.AddFrameworkInfrastructure();
        services.AddFrameworkMessaging();
        await using var provider = services.BuildServiceProvider();
        var lease = provider.GetRequiredService<IWorkLease>();

        var first = await lease.TryAcquireAsync("job-2", TimeSpan.FromSeconds(30));
        Assert.NotNull(first);
        await first!.DisposeAsync();

        await using var second = await lease.TryAcquireAsync("job-2", TimeSpan.FromSeconds(30));
        Assert.NotNull(second);
    }
}
