using Common.Application.Configuration.Options;
using Common.Application.Configuration.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using CoreQuant.Infrastructure.Persistence.EF.Configuration.Options;
using CoreQuant.Infrastructure.Persistence.EF.Configuration.Validators;
using CoreQuant.Infrastructure.Persistence.EF.Providers.MySql;

namespace CoreQuant.Infrastructure.Persistence.EF;

/// <summary>
/// Host-facing persistence composition (EF core + MySQL read/write contexts).
/// </summary>
public static class CoreQuantPersistenceHostExtensions
{
    /// <summary>
    /// Registers EF persistence, SlowQuery options, and MySQL <see cref="CoreQuantWriteDbContext"/> /
    /// <see cref="CoreQuantReadDbContext"/> from <c>Infrastructure:Databases</c>.
    /// </summary>
    public static IServiceCollection AddCoreQuantPersistenceEfMySql(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddCoreQuantPersistenceEf();

        services.AddOptions<OutboxOptions>()
            .Bind(configuration.GetSection(OutboxOptions.SectionName))
            .ValidateOnStart();
        services.TryAddSingleton<IValidateOptions<OutboxOptions>, OutboxOptionsValidator>();

        services.AddOptions<SlowQueryInterceptorOptions>()
            .Bind(configuration.GetSection(SlowQueryInterceptorOptions.SectionName))
            .ValidateOnStart();
        services.TryAddSingleton<IValidateOptions<SlowQueryInterceptorOptions>, SlowQueryInterceptorOptionsValidator>();

        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<DatabaseOptions>, DatabaseOptionsValidator>();

        services.AddMySqlReadWriteDbContexts<CoreQuantWriteDbContext, CoreQuantReadDbContext>(options =>
            configuration.GetSection(DatabaseOptions.SectionName).Bind(options));

        return services;
    }
}
