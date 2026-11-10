namespace Common.Domain.Exceptions;

/// <summary>
/// Standard domain-layer error codes.
/// </summary>
public static class DomainErrorCode
{
    public const string NotFound = "NOT_FOUND";
    public const string AlreadyExists = "ALREADY_EXISTS";
    public const string Duplicate = "DUPLICATE";
    public const string InvalidState = "INVALID_STATE";
    public const string InvalidStateTransition = "INVALID_STATE_TRANSITION";
    public const string AlreadyProcessed = "ALREADY_PROCESSED";
    public const string Expired = "EXPIRED";
    public const string InvariantViolation = "INVARIANT_VIOLATION";
    public const string ReferenceNotFound = "REFERENCE_NOT_FOUND";
    public const string IntegrityViolation = "INTEGRITY_VIOLATION";
    public const string ConcurrencyConflict = "CONCURRENCY_CONFLICT";
    public const string StaleVersion = "STALE_VERSION";
}
