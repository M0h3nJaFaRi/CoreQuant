using Common.Domain.Core;
using Common.Domain.Exceptions;
using Common.Domain.Guards;
using CoreQuant.Domain.Markets.Commodity.Aggregates.Instruments.Symbols.Events;
using CoreQuant.Domain.Markets.Commodity.Aggregates.Instruments.Symbols.ValueObjects;

namespace CoreQuant.Domain.Markets.Commodity.Aggregates.Instruments.Symbols.Entities;
public sealed class SymbolSession : Entity<Guid>
{
    private SymbolSession() { }

    public int DayOfWeek { get; private set; }
    public int SessionIndex { get; private set; }
    public TradingSession Period { get; private set; }
    public SymbolSessionType Type { get; private set; }
    public DateTimeOffset CapturedAtUtc { get; private set; }
    public SymbolSession(Guid id, SymbolSessionType type, int dayOfWeek, int sessionIndex, TradingSession period, DateTimeOffset capturedAtUtc)

        : base(id == Guid.Empty
            ? throw DomainException.InvalidValue(nameof(id))
            : id)
    {
        if (dayOfWeek is < 1 or > 7)
            throw DomainException.OutOfRange(nameof(dayOfWeek), 1, 7);

        if (sessionIndex < 0)
            throw DomainException.MustBeZeroOrPositive(nameof(sessionIndex));

        Type = type;
        DayOfWeek = dayOfWeek;
        SessionIndex = sessionIndex;
        Period = period ?? throw new ArgumentNullException(nameof(period));
        CapturedAtUtc = capturedAtUtc.ToUniversalTime();
    }

    public static SymbolSession Record(SymbolSessionType type, int dayOfWeek, int sessionIndex, TradingSession period, DateTimeOffset capturedAtUtc)
    {
        Ensure.That(dayOfWeek is >= 1 and <= 7, "Day of week must be between 1 and 7.", DomainExceptionCode.OutOfRange);
        Ensure.That(sessionIndex >= 0, "Session index cannot be negative.", DomainExceptionCode.MustBeZeroOrPositive);

        return new SymbolSession(Guid.NewGuid(), type, dayOfWeek, sessionIndex, period, capturedAtUtc);
    }
}