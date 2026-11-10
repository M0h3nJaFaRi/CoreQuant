namespace Common.Domain.Exceptions;

/// <summary>
/// Domain rule violation with an explicit bounded-context or module-specific error code.
/// </summary>
public sealed class DomainRuleException : DomainException
{
    public DomainRuleException(string message, string code, Exception? innerException = null)
        : base(code, message, innerException)
    {
    }
}
