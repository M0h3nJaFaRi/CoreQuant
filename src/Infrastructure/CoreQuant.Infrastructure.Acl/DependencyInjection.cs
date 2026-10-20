using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CoreQuant.Infrastructure.Acl.CurrentUser;
using Common.Application.Authorization.Abstraction;

namespace CoreQuant.Infrastructure.Acl;

/// <summary>
/// Registers service access control: HTTP user accessor, sample use-case policy, and the
/// opt-in development authentication scheme. Register in HTTP hosts only (Api) — Worker and
/// Scheduler keep the Common.Application null/allow-all defaults (no ambient HTTP user there).
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Replaces the null user accessor and allow-all policy with HTTP-backed implementations,
    /// and registers the <c>DevUser</c> scheme (honored only when <c>Infrastructure:Acl:DevAuthEnabled</c> is true).
    /// </summary>
    public static IServiceCollection AddAcl(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.RemoveAll<ICurrentUserAccessor>();
        services.AddScoped<ICurrentUserAccessor, HttpCurrentUserAccessor>();

        return services;
    }
}