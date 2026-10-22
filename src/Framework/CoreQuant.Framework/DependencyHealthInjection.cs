using Common.Framework.HealthChecks;
using Framework.Configurations.Options;
using Framework.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Framework;
public static class DependencyHealthInjection
{
    public static IHealthChecksBuilder AddDependencyHealthCheck(this IHealthChecksBuilder builder, string name, DependencyHealthOptions options, string kindTag, Func<IServiceProvider, DependencyHealthCheck> factory)
    {
        if (!options.Enabled)
            return builder;

        options.Validate(name);

        DependencyHealthCheck? shared = null;

        string[] tags = options.Critical ? [HealthTags.Ready, HealthTags.Startup, HealthTags.Detail, kindTag] : [HealthTags.Ready, HealthTags.Detail, kindTag];

        return builder.Add(new HealthCheckRegistration(name, sp => LazyInitializer.EnsureInitialized(ref shared, () => factory(sp)),
                           failureStatus: options.Critical ? HealthStatus.Unhealthy : HealthStatus.Degraded, tags: tags,
                           timeout: options.Timeout + TimeSpan.FromSeconds(2)));
    }
}