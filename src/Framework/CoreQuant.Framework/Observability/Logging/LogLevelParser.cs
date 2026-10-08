using Serilog.Events;

namespace Framework.Observability.Logging;

/// <summary>
/// Parses configuration log-level strings into Serilog <see cref="LogEventLevel"/>.
/// </summary>
internal static class LogLevelParser
{
    public static bool TryParse(string? value, out LogEventLevel level)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            level = LogEventLevel.Information;
            return false;
        }

        return Enum.TryParse(value.Trim(), ignoreCase: true, out level);
    }

    public static LogEventLevel ParseOrDefault(string? value, LogEventLevel fallback = LogEventLevel.Information)
        => TryParse(value, out var level) ? level : fallback;
}
