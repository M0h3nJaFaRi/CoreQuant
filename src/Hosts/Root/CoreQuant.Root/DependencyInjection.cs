using Common.Application;
using Common.Framework.HealthChecks;
using Common.Presentation;
using Common.Presentation.Api.Middlewares.ApiKeyHandling;
using Common.Presentation.Api.Middlewares.CorrelationHandling;
using Common.Presentation.Api.Middlewares.ExceptionHandling;
using Common.Presentation.Api.Middlewares.GlobalObservabilityHandling;
using Common.Presentation.Api.Middlewares.MaintenanceHandling;
using Common.Presentation.Api.Middlewares.RequestLoggingHandling;
using Common.Presentation.Contract;
using CoreQuant.Application;
using CoreQuant.Domain;
using CoreQuant.Infrastructure;
using CoreQuant.Infrastructure.Acl;
using CoreQuant.Infrastructure.Persistence.EF;
using Framework;
using Framework.HealthChecks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Reflection;

namespace CoreQuant.Root;
public static class DependencyInjection
{
    public static IServiceCollection AddApiDependency(this IServiceCollection services, IConfiguration configuration, Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddServiceIdentity(configuration, assembly, "CoreQuant", "Api")
                .AddFramework(configuration, typeof(Application.AssemblyMarker).Assembly)
                .AddApplication(configuration)
                .AddInfrastructure(configuration)
                .AddInfrastructureEf(configuration)
                .AddAcl(configuration)
                .AddDomain(configuration)
                .AddCommonApi(configuration)
                .AddCors()
                .AddOptions()
                .AddHttpContextAccessor()
                .AddProblemDetails()
                .AddHttpLogging(options =>
                {
                    options.LoggingFields = HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode | HttpLoggingFields.Duration;
                })
                .AddControllers();

        services.AddServiceHealth(configuration)
                .RegisterEfDatabaseCheck(configuration)
                .RegisterRedisHealthCheck(configuration);

        return services;
    }
    public static IServiceCollection AddConsumerDependency(this IServiceCollection services, IConfiguration configuration, Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddServiceIdentity(configuration, assembly, "CoreQuant", "Consumer")
                .AddFramework(configuration, typeof(Application.AssemblyMarker).Assembly)
                .AddApplication(configuration)
                .AddInfrastructure(configuration)
                .AddDomain(configuration);

        services.AddServiceHealth(configuration)
                .RegisterEfDatabaseCheck(configuration)
                .RegisterRedisHealthCheck(configuration);

        return services;
    }
    public static IServiceCollection AddSchedulerDependency(this IServiceCollection services, IConfiguration configuration, Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddServiceIdentity(configuration, assembly, "CoreQuant", "Scheduler")
                .AddFramework(configuration, typeof(Application.AssemblyMarker).Assembly)
                .AddApplication(configuration)
                .AddInfrastructure(configuration)
                .AddDomain(configuration);

        services.AddServiceHealth(configuration)
                .RegisterEfDatabaseCheck(configuration)
                .RegisterRedisHealthCheck(configuration);

        return services;
    }
    public static IServiceCollection AddWorkerDependency(this IServiceCollection services, IConfiguration configuration, Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddServiceIdentity(configuration, assembly, "CoreQuant", "Worker")
                .AddFramework(configuration, typeof(Application.AssemblyMarker).Assembly)
                .AddApplication(configuration)
                .AddInfrastructure(configuration)
                .AddDomain(configuration);

        services.AddServiceHealth(configuration)
                .RegisterEfDatabaseCheck(configuration)
                .RegisterRedisHealthCheck(configuration);

        return services;
    }

    public static IServiceProvider ValidateAssembly(this IServiceProvider serviceProvider)
    {
        AuthorizationPolicyValidator.ValidateAuthorizationPolicy(serviceProvider, typeof(Application.Contracts.AssemblyMarker).Assembly);

        return serviceProvider;
    }

    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        var identity = app.Services.GetRequiredService<ServiceIdentity>();

        app.UsePathBase($"/{identity.Namespace.Trim('/')}");    // 1. Must be first so every later middleware sees the stripped path
        app.UseMiddleware<CorrelationIdMiddleware>();           // 2. every later log/problem response gets the id
        app.UseMiddleware<GlobalObservabilityMiddleware>();     // 3. outside ExceptionHandling: sees final status + exception
        app.UseMiddleware<RequestLoggingMiddleware>();          // 4. outside ExceptionHandling: logs the final status
        app.UseMiddleware<ExceptionHandlingMiddleware>();       // 5. catches everything below, including routing errors
        app.UseRouting();                                       // 6. explicit, so routing runs INSIDE the exception handler
        app.UseCors();                                          // 7. after routing; preflight answered before 503/401
        app.UseMiddleware<MaintenanceModeMiddleware>();         // 8. cheap 503 before auth/timeouts/handlers
        // app.UseRateLimiter();                                // 9. optional: throttle before ApiKey to slow brute force
        app.UseRequestTimeouts();                               // 10. needs routing (endpoint metadata)
        app.UseMiddleware<ApiKeyMiddleware>();                  // 11. API key gate
        app.UseAuthentication();                                // 12. after routing, before authorization
        app.UseAuthorization();                                 // 13. reads endpoint metadata ([Authorize]/[AllowAnonymous])

        return app;
    }

    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        app.MapControllers();
        app.MapServiceHealth();

        return app;
    }
}