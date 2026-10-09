using Common.Application.Configuration.Options;
using Common.Application.Configuration.Validators;
using Common.Application.Outbox;
using Common.Application.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using CoreQuant.Infrastructure.Persistence.EF.Configuration.Options;
using CoreQuant.Infrastructure.Persistence.EF.Configuration.Validators;
using CoreQuant.Infrastructure.Persistence.EF.Interceptors;
using CoreQuant.Infrastructure.Persistence.EF.Interceptors.Observability;
using CoreQuant.Infrastructure.Persistence.EF.Outbox;
using CoreQuant.Infrastructure.Redis.Adapters.Events;

namespace CoreQuant.Infrastructure.Persistence.EF;

/// <summary>
/// Registers CoreQuant EF persistence implementations (UoW, interceptors; providers wired by host).
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Replaces default no-op UoW with EF implementation and registers EF interceptors.
    /// Call a provider extension (<c>AddMySql*</c> / <c>AddPostgreSql*</c> / <c>AddSqlServer*</c>) for DbContexts.
    /// </summary>
    public static IServiceCollection AddCoreQuantPersistenceEf(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<OutboxOptions>()
            .ValidateOnStart();
        services.TryAddSingleton<IValidateOptions<OutboxOptions>, OutboxOptionsValidator>();

        services.AddOptions<SlowQueryInterceptorOptions>()
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<SlowQueryInterceptorOptions>, SlowQueryInterceptorOptionsValidator>();

        services.RemoveAll<IUnitOfWork>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        services.RemoveAll<IOutboxPublisher>();
        services.TryAddSingleton<LoggingOutboxPublisher>();
        services.TryAddSingleton<InMemoryOutboxPublisher>();
        services.AddSingleton<IOutboxPublisher>(sp =>
        {
            var transport = sp.GetRequiredService<IOptions<OutboxOptions>>().Value.Transport;
            if (string.Equals(transport, "InMemory", StringComparison.OrdinalIgnoreCase))
            {
                return sp.GetRequiredService<InMemoryOutboxPublisher>();
            }

            if (string.Equals(transport, "Redis", StringComparison.OrdinalIgnoreCase))
            {
                return sp.GetRequiredService<RedisEventStoreOutboxPublisher>();
            }

            return sp.GetRequiredService<LoggingOutboxPublisher>();
        });

        services.TryAddScoped<SoftDeleteInterceptor>();
        services.TryAddScoped<AuditingSaveChangesInterceptor>();
        services.TryAddScoped<DomainEventOutboxInterceptor>();
        services.TryAddSingleton<RejectPhysicalDeleteInterceptor>();
        services.TryAddSingleton<ReadOnlyDbContextInterceptor>();

        services.TryAddSingleton<EfPersistenceMetrics>();
        services.TryAddSingleton<CommandInterceptor>();
        services.TryAddSingleton<MetricsInterceptor>();
        services.TryAddSingleton<SlowQueryInterceptor>();
        services.TryAddSingleton<ConnectionInterceptor>();
        services.TryAddSingleton<TransactionInterceptor>();

        return services;
    }
}
