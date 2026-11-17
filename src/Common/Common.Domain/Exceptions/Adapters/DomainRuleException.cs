using Common.Domain.Exceptions.Abstraction;

namespace Common.Domain.Exceptions.Adapters;

/// <summary>
/// Bounded-context rule violation identified by a module-specific error code string.
/// </summary>
public sealed class DomainRuleException(
    string message,
    string code,
    string? entityName = null,
    string? aggregateName = null,
    Exception? innerException = null)
    : DomainLayerException(code, message, entityName: entityName, aggregateName: aggregateName, innerException: innerException);
