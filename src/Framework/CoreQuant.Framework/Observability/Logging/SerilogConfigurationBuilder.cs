using Serilog;
using Serilog.Events;
using Serilog.Sinks.Syslog;
using Serilog.Sinks.SystemConsole.Themes;
using Framework.Observability.Configuration.Options;

namespace Framework.Observability.Logging;

/// <summary>
/// Applies <see cref="SerilogOptions"/> and <see cref="SyslogOptions"/> to a Serilog logger configuration.
/// </summary>
internal static class SerilogConfigurationBuilder
{
    public static void Configure(
        LoggerConfiguration loggerConfiguration,
        SerilogOptions serilogOptions,
        SyslogOptions syslogOptions,
        string applicationName)
    {
        ArgumentNullException.ThrowIfNull(loggerConfiguration);
        ArgumentNullException.ThrowIfNull(serilogOptions);
        ArgumentNullException.ThrowIfNull(syslogOptions);
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationName);

        var defaultLevel = LogLevelParser.ParseOrDefault(serilogOptions.MinimumLevel.Default);
        loggerConfiguration
            .MinimumLevel.Is(defaultLevel)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", applicationName);

        foreach (var (source, levelText) in serilogOptions.MinimumLevel.Override)
        {
            if (LogLevelParser.TryParse(levelText, out var level))
                loggerConfiguration.MinimumLevel.Override(source, level);
        }

        if (serilogOptions.Console.Enabled)
            loggerConfiguration.WriteTo.Console(theme: AnsiConsoleTheme.Code);

        if (syslogOptions.Enabled)
        {
            var restrictedTo = LogLevelParser.ParseOrDefault(
                syslogOptions.RestrictedToMinimumLevel,
                LogEventLevel.Information);

            var format = string.Equals(syslogOptions.Format, "RFC3164", StringComparison.OrdinalIgnoreCase)
                ? SyslogFormat.RFC3164
                : SyslogFormat.RFC5424;

            loggerConfiguration.WriteTo.UdpSyslog(
                host: syslogOptions.Address,
                port: syslogOptions.Port,
                appName: applicationName,
                format: format,
                restrictedToMinimumLevel: restrictedTo);
        }
    }
}
