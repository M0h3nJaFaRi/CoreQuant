using Common.Api.Swagger;
using Common.Infrastructure;
using Framework.Web;
using System.Reflection;
using Framework.Security;
using Common.Application;
using Framework.Validation;
using Framework.Observability;
using Microsoft.AspNetCore.Builder;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Api;

/// <summary>
/// Shared HTTP host composition for API processes.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers observability, Serilog/syslog logging, security, CQRS buses/behaviors, Redis cache/idempotency,
    /// and FluentValidation for the given application assemblies. Persistence is registered by the service host.
    /// </summary>
    /// <param name="serviceName">Fallback service name when <c>ApplicationName</c> is not configured.</param>
    public static IServiceCollection AddCommonApi(
        this IServiceCollection services,
        string serviceName,
        IConfiguration configuration,
        params Assembly[] applicationAssemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);
        ArgumentNullException.ThrowIfNull(configuration);

        var applicationName = ResolveApplicationName(configuration, serviceName);

        services.AddFrameworkObservability(options => options.ServiceName = applicationName);
        services.AddFrameworkWebObservability(configuration, applicationName);
        services.AddFrameworkLogging(configuration);
        services.AddFrameworkSecurity(configuration);
        services.AddCommonApplication(applicationAssemblies);
        services.AddCommonInfrastructureWithRedis(configuration);

        if (applicationAssemblies is { Length: > 0 })
            services.AddFrameworkValidation(applicationAssemblies);

        services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        services.AddCommonApiSwagger(configuration, applicationName);

        return services;
    }

    /// <summary>
    /// Adds Framework.Web middleware and maps the liveness health endpoint.
    /// </summary>
    public static WebApplication UseCommonApi(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseFrameworkWeb();
        app.UseCommonApiSwagger();
        app.MapFrameworkWebObservabilityEndpoints();
        return app;
    }

    private static string ResolveApplicationName(IConfiguration configuration, string fallback)
    {
        var configured = configuration["ApplicationName"];
        return string.IsNullOrWhiteSpace(configured) ? fallback : configured.Trim();
    }
}
