using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

namespace Framework.Resilience;

/// <summary>
/// Registers Framework.Resilience HTTP helpers. Product-specific pipelines do not belong here.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds the standard HTTP resilience handler (retry, timeout, circuit breaker) to <paramref name="builder"/>.
    /// </summary>
    public static IHttpClientBuilder AddFrameworkHttpResilience(this IHttpClientBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddStandardResilienceHandler();
        return builder;
    }
}
