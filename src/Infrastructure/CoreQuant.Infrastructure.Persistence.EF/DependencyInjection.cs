using Common.Framework.HealthChecks;
using Common.Infrastructure.Persistence.EF.DbContexts;
using Common.Infrastructure.Persistence.EF.UnitOfWork;
using CoreQuant.Infrastructure.Persistence.EF.Configuration;
using CoreQuant.Infrastructure.Persistence.EF.Configuration.Options;
using CoreQuant.Infrastructure.Persistence.EF.Configuration.Validators;
using CoreQuant.Infrastructure.Persistence.EF.DbContexts.Core;
using CoreQuant.Infrastructure.Persistence.EF.DbContexts.Markets.Commodity;
using CoreQuant.Infrastructure.Persistence.EF.DbContexts.Markets.Crypto;
using CoreQuant.Infrastructure.Persistence.EF.DbContexts.Markets.Forex;
using CoreQuant.Infrastructure.Persistence.EF.DbContexts.Markets.Indices;
using CoreQuant.Infrastructure.Persistence.EF.DbContexts.Markets.Metals;
using CoreQuant.Infrastructure.Persistence.EF.DbContexts.Markets.Stocks;
using CoreQuant.Infrastructure.Persistence.EF.DbContexts.News;
using CoreQuant.Infrastructure.Persistence.EF.Health;
using CoreQuant.Infrastructure.Persistence.EF.Interceptors;
using CoreQuant.Infrastructure.Persistence.EF.Interceptors.Observability;
using CoreQuant.Infrastructure.Persistence.EF.Observability;
using CoreQuant.Infrastructure.Persistence.EF.UnitOfWorks;
using Framework;
using Framework.Configurations.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Net.Mime;
using System.Reflection;
using System.Xml.Linq;

namespace CoreQuant.Infrastructure.Persistence.EF;

/// <summary>
/// Registers CoreQuant EF persistence implementations (UoW, interceptors; providers wired by host).
/// </summary>
public static class DependencyInjection
{
    private static readonly MySqlServerVersion DefaultServerVersion = new(new Version(8, 0, 36));

    /// <summary>
    /// Registers EF persistence, SlowQuery options, MySQL DbContexts from <c>Infrastructure:Databases</c>.
    /// </summary>
    public static IServiceCollection AddInfrastructureEf(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddCoreQuantInterceptor(configuration)
                       .AddCoreQuantDbContext(configuration)
                       .AddCoreQuantUnitOfWork(configuration)
                       .AddCoreQuantPersistenceObservability(configuration);
    }
    public static IHealthChecksBuilder RegisterEfDatabaseCheck(this IHealthChecksBuilder builder, IConfiguration configuration, params string[] databases)
    {
        var databaseOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>() ?? [];

        var registrations = databases.Length == 0 ? 
                            CoreQuantDbContextRegistration.All : 
                            [.. CoreQuantDbContextRegistration.All.Where(r => databases.Contains(r.ConfigurationKey, StringComparer.OrdinalIgnoreCase))];

        if (databases.Length > 0)
        {
            var unknown = databases.Except(CoreQuantDbContextRegistration.All.Select(r => r.ConfigurationKey), StringComparer.OrdinalIgnoreCase).ToList();

            if (unknown.Count > 0)
                throw new ArgumentException($"Unknown database key(s): {string.Join(", ", unknown)}.", nameof(databases));
        }

        foreach (var registration in registrations)
        {
            if (!databaseOptions.TryGetValue(registration.ConfigurationKey, out var endpoint))
                throw new InvalidOperationException($"Missing database configuration '{registration.ConfigurationKey}' at {DatabaseOptions.SectionName}:{registration.ConfigurationKey}.");

            if (!endpoint.IsConfigured) continue;

            if (!typeof(DbContext).IsAssignableFrom(registration.ContextType))
                throw new ArgumentException($"'{registration.ContextType.Name}' is not a DbContext.", nameof(registration.ContextType));

            builder.AddDependencyHealthCheck(name: $"{registration.ConfigurationKey.ToLowerInvariant()}-db", 
                                             options: endpoint.Health, kindTag: HealthTags.Db, 
                                             factory: sp => new EfDatabaseHealthCheck(sp.GetRequiredService<IServiceScopeFactory>(), registration.ContextType, endpoint.Health));
        }

        return builder;
    }

    internal static IServiceCollection AddCoreQuantUnitOfWork(this IServiceCollection services, IConfiguration configuration)
    {
        services.RemoveAll<IUnitOfWork>();

        services.AddScoped<EfUnitOfWork<NewsDbContext>>();
        services.AddScoped<EfUnitOfWork<ForexDbContext>>();
        services.AddScoped<EfUnitOfWork<StockDbContext>>();
        services.AddScoped<EfUnitOfWork<MetalDbContext>>();
        services.AddScoped<EfUnitOfWork<IndexDbContext>>();
        services.AddScoped<EfUnitOfWork<CryptoDbContext>>();
        services.AddScoped<EfUnitOfWork<CommodityDbContext>>();
        services.AddScoped<EfUnitOfWork<CoreQuantDbContext>>();

        services.AddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>();

        return services;
    }
    internal static IServiceCollection AddCoreQuantDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IValidateOptions<DatabaseOptions>, DatabaseOptionsValidator>();

        services.AddOptions<DatabaseOptions>()
                .Bind(configuration.GetSection(DatabaseOptions.SectionName))
                .ValidateOnStart();

        var databaseOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>() ?? [];

        foreach (var registration in CoreQuantDbContextRegistration.All)
            RegisterMySqlDbContext(services, databaseOptions, registration);

        return services;
    }
    internal static IServiceCollection AddCoreQuantInterceptor(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton<IValidateOptions<SlowQueryInterceptorOptions>, SlowQueryInterceptorOptionsValidator>();

        services.AddOptions<SlowQueryInterceptorOptions>()
                .Bind(configuration.GetSection(SlowQueryInterceptorOptions.SectionName))
                .ValidateOnStart();

        services.TryAddSingleton<SaveChangesMetricsInterceptor>();
        services.TryAddSingleton<CommandObservabilityInterceptor>();
        services.TryAddSingleton<ConnectionObservabilityInterceptor>();
        services.TryAddSingleton<TransactionObservabilityInterceptor>();

        return services;
    }
    internal static IServiceCollection AddCoreQuantPersistenceObservability(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton<EfMetrics>();

        return services;
    }
    private static void RegisterMySqlDbContext(IServiceCollection services, DatabaseOptions databaseOptions, CoreQuantDbContextRegistration.DbContextRegistration registration)
    {
        if (!databaseOptions.TryGetValue(registration.ConfigurationKey, out var endpoint))
            throw new InvalidOperationException($"Missing database configuration '{registration.ConfigurationKey}' at {DatabaseOptions.SectionName}:{registration.ConfigurationKey}.");

        var addMethod = typeof(DependencyInjection).GetMethod(nameof(AddMySqlDbContext), BindingFlags.NonPublic | BindingFlags.Static)!
                                                   .MakeGenericMethod(registration.ContextType);

        addMethod.Invoke(null, [services, endpoint]);
    }
    private static IServiceCollection AddMySqlDbContext<TContext>(this IServiceCollection services, DatabaseEndpointOptions endpoint)
    where TContext : BaseDbContext
    {
        var connectionString = endpoint.BuildMySqlConnectionString();
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<TContext>((sp, options) =>
        {
            options.UseMySql(connectionString, DefaultServerVersion, builder =>
            {
                if (endpoint.CommandTimeoutSeconds is { } timeout)
                    builder.CommandTimeout(timeout);
            });

            options.AddInterceptors(sp.GetRequiredService<SaveChangesMetricsInterceptor>());
            options.AddInterceptors(sp.GetRequiredService<CommandObservabilityInterceptor>());
            options.AddInterceptors(sp.GetRequiredService<ConnectionObservabilityInterceptor>());
            options.AddInterceptors(sp.GetRequiredService<TransactionObservabilityInterceptor>());
        });

        return services;
    }
}