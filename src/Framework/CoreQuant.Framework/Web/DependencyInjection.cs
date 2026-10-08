using Framework.Web.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Framework.Web;

/// <summary>
/// ASP.NET Core pipeline extensions for Framework.Web middleware.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds correlation id and HTTP metrics middleware. Requires <c>AddFrameworkObservability</c> for <see cref="System.Diagnostics.Metrics.Meter"/>.
    /// </summary>
    public static IApplicationBuilder UseFrameworkWeb(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<HttpMetricsMiddleware>();
        return app;
    }
}
