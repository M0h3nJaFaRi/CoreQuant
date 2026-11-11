using Common.Framework.Exceptions;

namespace Common.Domain.Exceptions.Abstraction;

/// <summary>
/// Base for domain invariant and rule violations. No HTTP or infrastructure dependencies.
/// </summary>
public abstract class DomainException(string code,
                                      string message,
                                      object? value = null,
                                      string? entityName = null,
                                      string? aggregateName = null,
                                      Exception? innerException = null) : Exception(message, innerException), IHasErrorCode
{
    public string Code { get; } = code.Trim();
    public object? Value { get; } = value;
    public string? EntityName { get; } = entityName;
    public string? AggregateName { get; } = aggregateName;
}