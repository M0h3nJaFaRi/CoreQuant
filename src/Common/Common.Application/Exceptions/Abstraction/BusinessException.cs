using Common.Framework.Exceptions;

namespace Common.Application.Exceptions.Abstraction;

/// <summary>
/// Base for application policy and business-rule violations (not domain invariants).
/// </summary>
public abstract class BusinessException(string code, string message, Exception? innerException = null)
    : Exception(message, innerException), IHasErrorCode
{
    public string Code { get; } = code.Trim();
}
