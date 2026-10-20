using Common.Domain.Core;

namespace CoreQuant.Domain.Markets.Commodity.Aggregates.Instruments.Symbols.ValueObjects;
public sealed class SymbolId : ValueObject
{
    public ulong Value { get; }

    public SymbolId(ulong value)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Symbol ID must be positive.");

        Value = value;
    }

    public override string ToString() => Value.ToString();
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}