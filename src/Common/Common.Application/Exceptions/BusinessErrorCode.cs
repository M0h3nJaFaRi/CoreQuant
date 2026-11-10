namespace Common.Application.Exceptions;

/// <summary>
/// Standard application-layer business error codes.
/// </summary>
public static class BusinessErrorCode
{
    public const string OperationNotAllowed = "OPERATION_NOT_ALLOWED";
    public const string LimitExceeded = "LIMIT_EXCEEDED";
    public const string InsufficientBalance = "INSUFFICIENT_BALANCE";
    public const string InsufficientQuantity = "INSUFFICIENT_QUANTITY";
    public const string BusinessRuleViolation = "BUSINESS_RULE_VIOLATION";
}
