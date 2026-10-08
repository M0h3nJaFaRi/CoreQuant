using Framework.Security.Hashing;
using Microsoft.Extensions.Options;
using Framework.Security.Encryption;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Framework.Security.Configuration.Options;
using Framework.Security.Configuration.Validators;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Framework.Security;

/// <summary>
/// Registers Framework.Security services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds hashing and encryption services with default (empty) encryption options.
    /// </summary>
    public static IServiceCollection AddFrameworkSecurity(this IServiceCollection services)
        => AddFrameworkSecurity(services, configuration: null);

    /// <summary>
    /// Adds hashing and encryption services, binding <see cref="EncryptionOptions"/> from
    /// <c>Framework:Encryption</c> when <paramref name="configuration"/> is provided.
    /// </summary>
    public static IServiceCollection AddFrameworkSecurity(
        this IServiceCollection services,
        IConfiguration? configuration)
    {
        ArgumentNullException.ThrowIfNull(services);

        var optionsBuilder = services.AddOptions<EncryptionOptions>()
            .ValidateOnStart();
        services.TryAddSingleton<IValidateOptions<EncryptionOptions>, EncryptionOptionsValidator>();

        if (configuration is not null)
            optionsBuilder.Bind(configuration.GetSection(EncryptionOptions.SectionName));

        services.TryAddSingleton<IHashService, HashService>();
        services.TryAddSingleton<IAesEncryptionService, AesEncryptionService>();
        services.TryAddSingleton<IRsaEncryptionService, RsaEncryptionService>();

        return services;
    }
}
