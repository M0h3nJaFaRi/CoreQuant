using System.Reflection;
using Common.Application.Authorization;
using Common.Application.Cqrs;
using Common.Application.Cqrs.Defaults;
using Common.Application.Cqrs.Abstractions;
using Common.Application.Cqrs.Behaviors.Authorization;
using Common.Application.Configuration.Options;
using Common.Application.Configuration.Validators;
using Common.Application.Cqrs.Behaviors.Caching;
using Common.Application.Cqrs.Behaviors.Idempotency;
using Common.Application.Cqrs.Behaviors.Logging;
using Common.Application.Cqrs.Behaviors.Metrics;
using Common.Application.Cqrs.Behaviors.Performance;
using Common.Application.Cqrs.Behaviors.RateLimit;
using Common.Application.Cqrs.Behaviors.Validation;
using Common.Application.Cqrs.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Common.Application;

/// <summary>
/// Registers Common.Application CQRS services and pipeline behaviors.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds command/query buses, default pipeline behaviors, and scans <paramref name="assemblies"/> for handlers.
    /// </summary>
    public static IServiceCollection AddCommonApplication(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<PerformanceBehaviorOptions>()
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<PerformanceBehaviorOptions>, PerformanceBehaviorOptionsValidator>();

        services.AddOptions<IdempotencyBehaviorOptions>()
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<IdempotencyBehaviorOptions>, IdempotencyBehaviorOptionsValidator>();

        services.AddOptions<CachingBehaviorOptions>()
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<CachingBehaviorOptions>, CachingBehaviorOptionsValidator>();

        services.AddOptions<RateLimitBehaviorOptions>()
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<RateLimitBehaviorOptions>, RateLimitBehaviorOptionsValidator>();

        services.TryAddScoped<ICurrentUserAccessor, NullCurrentUserAccessor>();
        services.TryAddScoped<IAuthorizationService, AllowAllAuthorizationService>();

        services.TryAddScoped<ICommandBus, CommandBus>();
        services.TryAddScoped<IQueryBus, QueryBus>();

        services.TryAddSingleton<CqrsMetrics>();

        services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>)));
        services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IPipelineBehavior<,>), typeof(MetricsBehavior<,>)));
        services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>)));
        services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>)));
        services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>)));
        services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IPipelineBehavior<,>), typeof(RateLimitBehavior<,>)));
        services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IPipelineBehavior<,>), typeof(IdempotencyBehavior<,>)));
        services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>)));

        if (assemblies is { Length: > 0 })
        {
            RegisterHandlers(services, assemblies);
        }

        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, Assembly[] assemblies)
    {
        var handlerTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsClass: true, IsAbstract: false });

        foreach (var type in handlerTypes)
        {
            foreach (var implemented in type.GetInterfaces())
            {
                if (!implemented.IsGenericType)
                {
                    continue;
                }

                var definition = implemented.GetGenericTypeDefinition();
                if (definition == typeof(ICommandHandler<>)
                    || definition == typeof(ICommandHandler<,>)
                    || definition == typeof(IQueryHandler<,>))
                {
                    services.TryAddEnumerable(ServiceDescriptor.Transient(implemented, type));
                }
            }
        }
    }
}
