using Common.Domain.Core;
using Common.Domain.Exceptions;
using Common.Domain.Guards;

namespace CoreQuant.Domain.Markets.Commodity.Aggregates.Instruments.Symbols.ValueObjects;
public sealed class SymbolName : ValueObject
{
    public string Value { get; }

    public SymbolName(string value)
    {
        Ensure.That(string.IsNullOrWhiteSpace(value), "Symbol name cannot be empty.", "out_of_range");
        
        Value = value.Trim();
    }

    public override string ToString() => Value;
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}