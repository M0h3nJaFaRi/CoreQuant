using Framework;
using CoreQuant.Domain;
using System.Reflection;
using Common.Application;
using Common.Presentation;
using CoreQuant.Application;
using CoreQuant.Infrastructure;
using CoreQuant.Infrastructure.Acl;
using Microsoft.Extensions.Configuration;
using CoreQuant.Infrastructure.Persistence.EF;
using Microsoft.Extensions.DependencyInjection;

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
                .AddDomain(configuration);

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
}