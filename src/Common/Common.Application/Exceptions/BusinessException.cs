using Common.Framework.Exceptions;

namespace Common.Application.Exceptions;

/// <summary>
/// Base for application policy and business-rule violations (not domain invariants).
/// </summary>
public abstract class BusinessException : Exception, IHasErrorCode
{
    protected BusinessException(string code, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        Code = code.Trim();
    }

    public string Code { get; }
}
