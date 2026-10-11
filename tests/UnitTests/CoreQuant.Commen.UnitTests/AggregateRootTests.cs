using Common.Domain.Core;
using Common.Domain.Events;

namespace CoreQuant.Commen.UnitTests;

public sealed class AggregateRootTests
{
    [Fact]
    public void RaiseDomainEvent_Collects_Events()
    {
        var aggregate = SampleAggregate.Create(1);
        Assert.Single(aggregate.DomainEvents);
        Assert.IsType<SampleCreated>(aggregate.DomainEvents.First());
    }

    [Fact]
    public void MarkAsRemoved_Sets_SoftDelete()
    {
        var aggregate = SampleAggregate.Create(2);
        aggregate.MarkAsRemoved(99ul);

        Assert.True(aggregate.IsDeleted);
        Assert.Equal(99ul, aggregate.DeletedBy);
        Assert.NotNull(aggregate.DeletedAt);
    }

    private sealed class SampleAggregate : AggregateRoot<uint>
    {
        private SampleAggregate(uint id): base(id) { }

        public static SampleAggregate Create(uint id)
        {
            var aggregate = new SampleAggregate(id);
            aggregate.RaiseDomainEvent(new SampleCreated(id));
            return aggregate;
        }
    }

    private sealed record SampleCreated(uint Id) : DomainEvent;
}
