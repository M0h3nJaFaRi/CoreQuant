using Common.Framework.Exceptions.Abstraction;

namespace Common.Domain.Exceptions.Abstraction;

/// <summary>
/// Base for domain invariant and rule violations. No HTTP or infrastructure dependencies.
/// </summary>
public abstract class DomainLayerException(string errorCode, string message, object? value = null, string? entityName = null, string? aggregateName = null, Exception? innerException = null) : 
                      ServiceException(errorCode: errorCode, message: message, innerException: innerException)
{
    public object? Value { get; } = value;
    public string? EntityName { get; } = entityName;
    public string? AggregateName { get; } = aggregateName;
}