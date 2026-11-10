using Common.Framework.Exceptions;

namespace Common.Domain.Exceptions;

/// <summary>
/// Base for domain invariant and rule violations. No HTTP or infrastructure dependencies.
/// </summary>
public abstract class DomainException : Exception, IHasErrorCode
{
    protected DomainException(
        string code,
        string message,
        Exception? innerException = null,
        object? value = null,
        string? entityName = null,
        string? aggregateName = null)
        : base(message, innerException)
    {
        Code = code.Trim();
        Value = value;
        EntityName = entityName;
        AggregateName = aggregateName;
    }

    public string Code { get; }

    public object? Value { get; }

    public string? EntityName { get; }

    public string? AggregateName { get; }
}
