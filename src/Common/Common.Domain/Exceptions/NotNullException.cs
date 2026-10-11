namespace Common.Domain.Exceptions;
public sealed class NotNullException(string message, string code = "not_null") : DomainException(message, code);