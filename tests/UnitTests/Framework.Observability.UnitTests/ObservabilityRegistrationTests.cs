using System.Diagnostics;
using System.Diagnostics.Metrics;
using Framework.Observability;
using Framework.Observability.Configuration.Options;
using Framework.Observability.Correlation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Framework.Observability.UnitTests;

public sealed class ObservabilityRegistrationTests
{
    [Fact]
    public void CorrelationKeys_Are_Stable()
    {
        Assert.Equal("correlation.id", CorrelationKeys.CorrelationId);
        Assert.Equal("X-Correlation-ID", CorrelationKeys.HeaderName);
    }

    [Fact]
    public void AddFrameworkObservability_Registers_Instruments_And_Health()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFrameworkObservability(o => o.ServiceName = "test-service");
        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<FrameworkObservabilityOptions>>().Value;
        var activitySource = provider.GetRequiredService<ActivitySource>();
        var meter = provider.GetRequiredService<Meter>();
        var health = provider.GetRequiredService<HealthCheckService>();

        Assert.Equal("test-service", options.ServiceName);
        Assert.Equal("test-service", activitySource.Name);
        Assert.Equal("test-service", meter.Name);
        Assert.NotNull(health);
    }
}
