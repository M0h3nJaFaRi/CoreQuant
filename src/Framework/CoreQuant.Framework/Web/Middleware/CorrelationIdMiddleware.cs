using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Framework.Observability.Correlation;

namespace Framework.Web.Middleware;

/// <summary>
/// Ensures each request has a correlation id (header + activity tag).
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var correlationId = ResolveCorrelationId(context);
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationKeys.HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        context.Items[CorrelationKeys.CorrelationId] = correlationId;
        Activity.Current?.SetTag(CorrelationKeys.CorrelationId, correlationId);

        await next(context).ConfigureAwait(false);
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationKeys.HeaderName, out StringValues incoming)
            && !StringValues.IsNullOrEmpty(incoming))
            return incoming.ToString();

        return Guid.NewGuid().ToString("N");
    }
}
