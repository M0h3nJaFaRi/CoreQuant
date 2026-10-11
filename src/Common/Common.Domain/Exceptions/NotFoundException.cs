namespace Common.Domain.Exceptions;

/// <summary>
/// Thrown when a required domain object cannot be found.
/// </summary>
public sealed class NotFoundException(string message, string code = "not_found") : DomainException(message, code);