using System.Diagnostics;
using Common.Application.Outbox;
using Common.Domain.Core;
using Common.Domain.Events;
using Framework.Observability.Correlation;
using Framework.Serialization.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
namespace CoreQuant.Infrastructure.Persistence.EF.Interceptors;

/// <summary>
/// Collects domain events from tracked aggregate roots, writes them to the transactional outbox,
/// and clears the in-memory event list — all within the same SaveChanges transaction.
/// </summary>
/// <remarks>
/// The <see cref="IOutboxWriter"/> is resolved lazily at <c>SaveChanges</c> time (not in the
/// constructor) because the writer's repository needs the same <see cref="DbContext"/> whose
/// options are still being built while this interceptor is constructed. Eager resolution
/// would couple context construction to repository resolution.
/// </remarks>
internal sealed class DomainEventOutboxInterceptor : SaveChangesInterceptor
{
    private readonly Lazy<IOutboxWriter> _outboxWriter;
    private readonly IJsonSerializer _serializer;

    public DomainEventOutboxInterceptor(IServiceProvider serviceProvider, IJsonSerializer serializer)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(serializer);
        _outboxWriter = new Lazy<IOutboxWriter>(serviceProvider.GetRequiredService<IOutboxWriter>);
        _serializer = serializer;
    }

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        WriteDomainEvents(eventData.Context, CancellationToken.None, requireSynchronousCompletion: true);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc />
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        await WriteDomainEventsAsync(eventData.Context, cancellationToken).ConfigureAwait(false);
        return await base.SavingChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false);
    }

    private void WriteDomainEvents(
        DbContext? context,
        CancellationToken cancellationToken,
        bool requireSynchronousCompletion)
    {
        if (context is null)
        {
            return;
        }

        var correlationId = ResolveCorrelationId();

        foreach (var aggregate in GetTrackedAggregates(context))
        {
            var domainEvents = GetDomainEvents(aggregate).ToList();
            if (domainEvents.Count == 0)
            {
                continue;
            }

            foreach (var domainEvent in domainEvents)
            {
                var task = _outboxWriter.Value.WriteAsync(CreateWrite(domainEvent, correlationId), cancellationToken);
                if (requireSynchronousCompletion && !task.IsCompletedSuccessfully)
                {
                    throw new InvalidOperationException(
                        $"{nameof(IOutboxWriter)} must complete synchronously during {nameof(SavingChanges)}.");
                }
            }

            ClearDomainEvents(aggregate);
        }
    }

    private async Task WriteDomainEventsAsync(DbContext? context, CancellationToken cancellationToken)
    {
        if (context is null)
        {
            return;
        }

        var correlationId = ResolveCorrelationId();

        foreach (var aggregate in GetTrackedAggregates(context))
        {
            var domainEvents = GetDomainEvents(aggregate).ToList();
            if (domainEvents.Count == 0)
            {
                continue;
            }

            foreach (var domainEvent in domainEvents)
            {
                await _outboxWriter.Value
                    .WriteAsync(CreateWrite(domainEvent, correlationId), cancellationToken)
                    .ConfigureAwait(false);
            }

            ClearDomainEvents(aggregate);
        }
    }

    private OutboxMessageWrite CreateWrite(IDomainEvent domainEvent, string? correlationId)
    {
        var messageType = domainEvent.GetType().FullName ?? domainEvent.GetType().Name;

        return new OutboxMessageWrite
        {
            MessageId = domainEvent.EventId,
            MessageType = messageType,
            // Cast to object so serialization uses the runtime event type: the generic
            // overload would serialize only the IDomainEvent interface members and drop
            // every event-specific field consumers rely on.
            Payload = _serializer.Serialize((object)domainEvent),
            OccurredOnUtc = domainEvent.OccurredOnUtc,
            SchemaVersion = domainEvent.SchemaVersion,
            CorrelationId = correlationId,
        };
    }

    private static IReadOnlyList<object> GetTrackedAggregates(DbContext context) =>
        // Materialized upfront: writing outbox rows below adds entities to the tracker,
        // which would invalidate a lazy enumeration over ChangeTracker.Entries().
        context.ChangeTracker.Entries()
            .Select(e => e.Entity)
            .Where(IsBusinessAggregateRoot)
            .Distinct()
            .ToList();

    private static string? ResolveCorrelationId()
    {
        var activity = Activity.Current;
        if (activity is null)
        {
            return null;
        }

        var tagged = activity.GetTagItem(CorrelationKeys.CorrelationId)?.ToString();
        if (!string.IsNullOrWhiteSpace(tagged))
        {
            return tagged;
        }

        return activity.Id;
    }

    /// <summary>
    /// Outbox messages are aggregates but must not be scanned for domain events (avoids recursion).
    /// </summary>
    private static bool IsBusinessAggregateRoot(object entity)
    {
        if (string.Equals(entity.GetType().Name, "OutboxMessage", StringComparison.Ordinal))
        {
            return false;
        }

        for (var type = entity.GetType(); type is not null; type = type.BaseType)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(AggregateRoot<>))
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<IDomainEvent> GetDomainEvents(object aggregate)
    {
        var aggregateRootType = FindAggregateRootBase(aggregate.GetType());
        if (aggregateRootType is null)
        {
            return [];
        }

        var property = aggregateRootType.GetProperty(nameof(AggregateRoot<long>.DomainEvents));
        if (property?.GetValue(aggregate) is not IEnumerable<IDomainEvent> events)
        {
            return [];
        }

        return events;
    }

    private static void ClearDomainEvents(object aggregate)
    {
        var aggregateRootType = FindAggregateRootBase(aggregate.GetType());
        aggregateRootType?
            .GetMethod(nameof(AggregateRoot<long>.ClearDomainEvents))?
            .Invoke(aggregate, null);
    }

    private static Type? FindAggregateRootBase(Type type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(AggregateRoot<>))
            {
                return current;
            }
        }

        return null;
    }
}
