using Common.Domain.Core;
using Common.Domain.Exceptions;

namespace CoreQuant.Domain.Markets.Commodity.Aggregates.Instruments.Symbols.ValueObjects;

public sealed class TradingSession : ValueObject
{
    public TimeOnly To { get; }
    public TimeOnly From { get; }

    public TradingSession(TimeOnly from, TimeOnly to)
    {
        if (from == to)
            throw DomainException.Guard("Session start and end times cannot be equal.", "range_equal");

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