namespace Common.Domain.Exceptions;
public sealed class NotEmptyException(string message, string code = "not_empty") : DomainException(message, code);