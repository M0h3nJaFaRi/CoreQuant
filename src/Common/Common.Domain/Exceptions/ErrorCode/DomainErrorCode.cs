namespace Common.Domain.Exceptions.ErrorCode;

internal static class DomainErrorCode
{
    public const string NotFound = "NotFound";
    public const string AlreadyExists = "AlreadyExists";
    public const string Duplicate = "Duplicate";
    public const string InvalidState = "InvalidState";
    public const string InvalidStateTransition = "InvalidStateTransition";
    public const string AlreadyProcessed = "AlreadyProcessed";
    public const string Expired = "Expired";
    public const string InvariantViolation = "InvariantViolation";
    public const string ReferenceNotFound = "ReferenceNotFound";
    public const string IntegrityViolation = "IntegrityViolation";
    public const string ConcurrencyConflict = "ConcurrencyConflict";
    public const string StaleVersion = "StaleVersion";
}
