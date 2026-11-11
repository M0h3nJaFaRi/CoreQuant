using Common.Domain.Core;
using Common.Domain.Exceptions.Adapters;

namespace CoreQuant.Domain.Markets.Commodity.Aggregates.Instruments.Symbols.ValueObjects;

public sealed class TradingSession : ValueObject
{
    public TimeOnly To { get; }
    public TimeOnly From { get; }

    public TradingSession(TimeOnly from, TimeOnly to)
    {
        if (from == to)
            throw new DomainRuleException("Session start and end times cannot be equal.", "RangeEqual");

        From = from;
        To = to;
    }

    public bool CrossesMidnight => To < From;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return From;
        yield return To;
    }
}