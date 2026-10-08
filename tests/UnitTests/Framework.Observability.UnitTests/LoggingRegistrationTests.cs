using Framework.Observability.Configuration.Options;
using Framework.Observability.Configuration.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Framework.Observability.UnitTests;

public sealed class LoggingRegistrationTests
{
    [Fact]
    public void AddFrameworkLogging_Binds_Serilog_And_Syslog_Options()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Framework:Serilog:MinimumLevel:Default"] = "Debug",
                ["Framework:Serilog:Console:Enabled"] = "true",
                ["Framework:Syslog:Enabled"] = "false",
                ["Framework:Syslog:Format"] = "RFC5424",
                ["Framework:Syslog:RestrictedToMinimumLevel"] = "Information"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFrameworkObservability(o => o.ServiceName = "logging-test");
        services.AddFrameworkLogging(configuration);

        using var provider = services.BuildServiceProvider();
        var serilog = provider.GetRequiredService<IOptions<SerilogOptions>>().Value;
        var syslog = provider.GetRequiredService<IOptions<SyslogOptions>>().Value;
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("test");

        Assert.Equal("Debug", serilog.MinimumLevel.Default);
        Assert.True(serilog.Console.Enabled);
        Assert.False(syslog.Enabled);
        logger.LogInformation("serilog wired");
    }

    [Fact]
    public void SyslogOptionsValidator_Requires_Address_When_Enabled()
    {
        var validator = new SyslogOptionsValidator();
        var result = validator.Validate(null, new SyslogOptions
        {
            Enabled = true,
            Address = "",
            Port = 514,
            Format = "RFC5424",
            RestrictedToMinimumLevel = "Information"
        });

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void SerilogOptionsValidator_Rejects_Invalid_Level()
    {
        var validator = new SerilogOptionsValidator();
        var result = validator.Validate(null, new SerilogOptions
        {
            MinimumLevel = new SerilogMinimumLevelOptions { Default = "Nope" }
        });

        Assert.False(result.Succeeded);
    }
}
