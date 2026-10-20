using Common.Presentation.Contract;
using Framework.Observability.Configuration.Options;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Syslog;
using Serilog.Sinks.SystemConsole.Themes;

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
        ServiceIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(loggerConfiguration);
        ArgumentNullException.ThrowIfNull(serilogOptions);
        ArgumentNullException.ThrowIfNull(syslogOptions);

        var defaultLevel = LogLevelParser.ParseOrDefault(serilogOptions.MinimumLevel.Default);
        
        loggerConfiguration.MinimumLevel.Is(defaultLevel)
                           .Enrich.FromLogContext()
                           .Enrich.WithProperty("Application", identity.Name)
                           .Enrich.WithProperty("Namespace", identity.Namespace)
                           .Enrich.WithProperty("Component", identity.Component)
                           .Enrich.WithProperty("Version", identity.Version);

        foreach (var (source, levelText) in serilogOptions.MinimumLevel.Override)
        {
            if (LogLevelParser.TryParse(levelText, out var level))
                loggerConfiguration.MinimumLevel.Override(source, level);
        }

        if (serilogOptions.Console.Enabled)
            loggerConfiguration.WriteTo.Console(theme: AnsiConsoleTheme.Code);

        if (syslogOptions.Enabled)
        {
            var restrictedTo = LogLevelParser.ParseOrDefault(syslogOptions.RestrictedToMinimumLevel, LogEventLevel.Information);

            var format = string.Equals(syslogOptions.Format, "RFC3164", StringComparison.OrdinalIgnoreCase)
                ? SyslogFormat.RFC3164
                : SyslogFormat.RFC5424;

            loggerConfiguration.WriteTo.UdpSyslog(format: format,
                                                  appName: identity.Name,
                                                  port: syslogOptions.Port,
                                                  host: syslogOptions.Address,
                                                  restrictedToMinimumLevel: restrictedTo);
        }
    }
}
