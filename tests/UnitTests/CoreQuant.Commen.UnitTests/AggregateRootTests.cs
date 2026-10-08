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
        aggregate.MarkAsRemoved(99);

        Assert.True(aggregate.IsDeleted);
        Assert.Equal(99, aggregate.DeletedBy);
        Assert.NotNull(aggregate.DeletedAt);
    }

    private sealed class SampleAggregate : AggregateRoot<int>
    {
        private SampleAggregate(int id)
            : base(id)
        {
        }

        public static SampleAggregate Create(int id)
        {
            var aggregate = new SampleAggregate(id);
            aggregate.RaiseDomainEvent(new SampleCreated(id));
            return aggregate;
        }
    }

    private sealed record SampleCreated(int Id) : DomainEvent;
}
