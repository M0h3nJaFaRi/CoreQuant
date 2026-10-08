using Common.Application.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using CoreQuant.Infrastructure.Acl.Authentication;
using CoreQuant.Infrastructure.Acl.Authorization;
using CoreQuant.Infrastructure.Acl.Configuration.Options;
using CoreQuant.Infrastructure.Acl.Configuration.Validators;
using CoreQuant.Infrastructure.Acl.CurrentUser;

namespace CoreQuant.Infrastructure.Acl;

/// <summary>
/// Registers service access control: HTTP user accessor, sample use-case policy, and the
/// opt-in development authentication scheme. Register in HTTP hosts only (Api) — Worker and
/// Scheduler keep the Common.Application null/allow-all defaults (no ambient HTTP user there).
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Name of the development authentication scheme.
    /// </summary>
    public const string DevUserSchemeName = "DevUser";

    /// <summary>
    /// Replaces the null user accessor and allow-all policy with HTTP-backed implementations,
    /// and registers the <c>DevUser</c> scheme (honored only when <c>Infrastructure:Acl:DevAuthEnabled</c> is true).
    /// </summary>
    public static IServiceCollection AddCoreQuantAcl(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<AclOptions>()
            .Bind(configuration.GetSection(AclOptions.SectionName))
            .ValidateOnStart();
        services.TryAddSingleton<IValidateOptions<AclOptions>, AclOptionsValidator>();

        var acl = configuration.GetSection(AclOptions.SectionName).Get<AclOptions>() ?? new AclOptions();

        services.AddHttpContextAccessor();

        services.RemoveAll<ICurrentUserAccessor>();
        services.AddScoped<ICurrentUserAccessor, HttpCurrentUserAccessor>();

        services.RemoveAll<IAuthorizationService>();
        services.AddScoped<IAuthorizationService, SampleAuthorizationService>();

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = DevUserSchemeName;
                options.DefaultChallengeScheme = DevUserSchemeName;
            })
            .AddScheme<DevUserAuthenticationOptions, DevUserAuthenticationHandler>(
                DevUserSchemeName,
                options => options.Enabled = acl.DevAuthEnabled);

        return services;
    }
}
