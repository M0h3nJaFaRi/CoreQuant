using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CoreQuant.Infrastructure.Persistence.EF.Configuration.Options;
using CoreQuant.Infrastructure.Persistence.EF.Interceptors;

namespace CoreQuant.Infrastructure.Persistence.EF.Providers.PostgreSql;

/// <summary>
/// PostgreSQL read/write DbContext registration (separate endpoints).
/// </summary>
public static class PostgreSqlPersistenceExtensions
{
    /// <summary>
    /// Registers a PostgreSQL <see cref="WriteDbContext"/> against the primary database.
    /// Call <c>AddCoreQuantPersistenceEf</c> before this so interceptors are registered.
    /// </summary>
    public static IServiceCollection AddPostgreSqlWriteDbContext<TContext>(
        this IServiceCollection services,
        Action<DatabaseEndpointOptions> configure)
        where TContext : WriteDbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var endpoint = new DatabaseEndpointOptions();
        configure(endpoint);
        var connectionString = endpoint.BuildNpgsqlConnectionString();
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<TContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, builder =>
            {
                if (endpoint.CommandTimeoutSeconds is { } timeout)
                {
                    builder.CommandTimeout(timeout);
                }
            });
            EfInterceptorRegistration.AddWriteInterceptors(options, sp);
        });

        services.TryAddScoped<WriteDbContext>(sp => sp.GetRequiredService<TContext>());
        services.TryAddScoped<MainDbContext>(sp => sp.GetRequiredService<TContext>());
        return services;
    }

    /// <summary>
    /// Registers a PostgreSQL <see cref="ReadDbContext"/> against the read replica (or primary when unset).
    /// Call <c>AddCoreQuantPersistenceEf</c> before this so interceptors are registered.
    /// </summary>
    public static IServiceCollection AddPostgreSqlReadDbContext<TContext>(
        this IServiceCollection services,
        Action<DatabaseEndpointOptions> configure)
        where TContext : ReadDbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var endpoint = new DatabaseEndpointOptions();
        configure(endpoint);
        var connectionString = endpoint.BuildNpgsqlConnectionString();
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<TContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, builder =>
            {
                if (endpoint.CommandTimeoutSeconds is { } timeout)
                {
                    builder.CommandTimeout(timeout);
                }
            });
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            EfInterceptorRegistration.AddReadInterceptors(options, sp);
        });

        services.TryAddScoped<ReadDbContext>(sp => sp.GetRequiredService<TContext>());
        return services;
    }

    /// <summary>
    /// Registers PostgreSQL write and read DbContexts with separate endpoints.
    /// </summary>
    public static IServiceCollection AddPostgreSqlReadWriteDbContexts<TWriteContext, TReadContext>(
        this IServiceCollection services,
        Action<DatabaseOptions> configure)
        where TWriteContext : WriteDbContext
        where TReadContext : ReadDbContext
    {
        ArgumentNullException.ThrowIfNull(configure);

        var options = new DatabaseOptions();
        configure(options);
        var read = options.ResolveReadEndpoint();

        services.AddPostgreSqlWriteDbContext<TWriteContext>(w => w.CopyFrom(options.Write));
        services.AddPostgreSqlReadDbContext<TReadContext>(r => r.CopyFrom(read));

        return services;
    }
}
