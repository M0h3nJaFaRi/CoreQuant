using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Framework.HealthChecks;
internal static class HealthResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },

    };

    public static Task Minimal(HttpContext context, HealthReport report) => WriteAsync(context, report, includeDetails: false);
    public static Task Detailed(HttpContext context, HealthReport report) => WriteAsync(context, report, includeDetails: true);
    
    private static Task WriteAsync(HttpContext context, HealthReport report, bool includeDetails)
    {
        object payload = includeDetails ? BuildDetailed(report) : BuildMinimal(report);
        return context.Response.WriteAsJsonAsync(payload, JsonOptions);
    }
    private static object BuildMinimal(HealthReport report) => new
    {
        status = report.Status,
        totalMs = (long)report.TotalDuration.TotalMilliseconds,
        checks = report.Entries.Select(e => new
        {
            name = e.Key,
            status = e.Value.Status,
            ms = (long)e.Value.Duration.TotalMilliseconds
        })
    };
    private static object BuildDetailed(HealthReport report) => new
    {
        status = report.Status,
        totalMs = (long)report.TotalDuration.TotalMilliseconds,
        checks = report.Entries.Select(e => new
        {
            name = e.Key,
            status = e.Value.Status,
            ms = (long)e.Value.Duration.TotalMilliseconds,
            description = e.Value.Description,
            tags = e.Value.Tags,
            data = e.Value.Data.Count == 0 ? null : e.Value.Data.ToDictionary(d => d.Key, d => d.Value?.ToString()),
            error = e.Value.Exception is null ? null : new
            {
                type = e.Value.Exception.GetType().Name,
                message = e.Value.Exception.Message
            }
        })
    };
}