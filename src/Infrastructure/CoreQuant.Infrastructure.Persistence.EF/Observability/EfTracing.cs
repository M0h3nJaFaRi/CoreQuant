using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace CoreQuant.Infrastructure.Persistence.EF.Observability;
internal static class EfTracing
{
    public static Activity? StartCompleted(this ActivitySource source, string name, DateTimeOffset start, TimeSpan duration)
    {
        var activity = source.StartActivity(name, ActivityKind.Client);
        
        if (activity is null)
            return null;

        var startUtc = start.UtcDateTime;
        activity.SetStartTime(startUtc);
        activity.SetEndTime(startUtc + duration);
        
        return activity;
    }

    public static string DbSystem(DbContext? context)
    {
        var provider = context?.Database.ProviderName;
        
        if (provider is null) return "other_sql";

        if (provider.Contains("Npgsql", StringComparison.OrdinalIgnoreCase)) return "postgresql";
        if (provider.Contains("SqlServer", StringComparison.OrdinalIgnoreCase)) return "sql_server";
        if (provider.Contains("MySql", StringComparison.OrdinalIgnoreCase)) return "mysql";
        if (provider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase)) return "sqlite";
        return "other_sql";
    }
}