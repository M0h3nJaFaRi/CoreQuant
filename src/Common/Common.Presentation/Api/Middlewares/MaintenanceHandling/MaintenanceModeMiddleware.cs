using Common.Presentation.Api.Configuration.Options;
using Common.Presentation.Api.Extensions;
using Common.Presentation.Api.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace Common.Presentation.Api.Middlewares.MaintenanceHandling;

public sealed class MaintenanceModeMiddleware(IOptionsMonitor<MaintenanceOptions> optionsMonitor) : IMiddleware
{
    private const string DefaultMessage = "Service is temporarily unavailable.";

    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Read once: CurrentValue can change between reads when the config reloads.
        var options = optionsMonitor.CurrentValue;

        if (!options.Enabled || PathMatcher.IsExcluded(context.Request.Path, options.ExcludedPaths))
            return next(context);

        return WriteMaintenanceResponseAsync(context, options);
    }

    private static Task WriteMaintenanceResponseAsync(HttpContext context, MaintenanceOptions options)
    {
        var headers = context.Response.Headers;
        headers.RetryAfter = Math.Max(0, options.RetryAfterSeconds).ToString(CultureInfo.InvariantCulture);
        headers.CacheControl = "no-store";

        var problem = new ApiProblemDetails
        {
            Type = "urn:error:maintenance",
            Title = "Service Unavailable",
            Status = StatusCodes.Status503ServiceUnavailable,
            Detail = string.IsNullOrWhiteSpace(options.Message) ? DefaultMessage : options.Message,
            Instance = context.Request.Path.Value
        };

        return context.WriteProblemAsync(problem);
    }
}
