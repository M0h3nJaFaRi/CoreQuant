using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using CoreQuant.Infrastructure.Persistence.EF.Interceptors.Observability;

namespace CoreQuant.Infrastructure.Persistence.EF.Interceptors;

/// <summary>
/// Resolves ordered EF interceptors for write and read DbContexts.
/// </summary>
internal static class EfInterceptorRegistration
{
    /// <summary>
    /// SoftDelete → Auditing → DomainEvent/Outbox → RejectPhysicalDelete + observability interceptors.
    /// </summary>
    public static IInterceptor[] ResolveWrite(IServiceProvider sp) =>
    [
        sp.GetRequiredService<SoftDeleteInterceptor>(),
        sp.GetRequiredService<AuditingSaveChangesInterceptor>(),
        sp.GetRequiredService<DomainEventOutboxInterceptor>(),
        sp.GetRequiredService<RejectPhysicalDeleteInterceptor>(),
        sp.GetRequiredService<CommandInterceptor>(),
        sp.GetRequiredService<MetricsInterceptor>(),
        sp.GetRequiredService<SlowQueryInterceptor>(),
        sp.GetRequiredService<ConnectionInterceptor>(),
        sp.GetRequiredService<TransactionInterceptor>(),
    ];

    /// <summary>
    /// Read-only guard + observability interceptors (no mutating SaveChanges pipeline).
    /// </summary>
    public static IInterceptor[] ResolveRead(IServiceProvider sp) =>
    [
        sp.GetRequiredService<ReadOnlyDbContextInterceptor>(),
        sp.GetRequiredService<CommandInterceptor>(),
        sp.GetRequiredService<MetricsInterceptor>(),
        sp.GetRequiredService<SlowQueryInterceptor>(),
        sp.GetRequiredService<ConnectionInterceptor>(),
        sp.GetRequiredService<TransactionInterceptor>(),
    ];

    public static void AddWriteInterceptors(DbContextOptionsBuilder options, IServiceProvider sp) =>
        options.AddInterceptors(ResolveWrite(sp));

    public static void AddReadInterceptors(DbContextOptionsBuilder options, IServiceProvider sp) =>
        options.AddInterceptors(ResolveRead(sp));
}
