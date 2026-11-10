namespace Common.Application.Exceptions;

/// <summary>
/// Standard validation error codes.
/// </summary>
public static class ValidationErrorCode
{
    public const string ValidationFailed = "VALIDATION_FAILED";
    public const string Required = "REQUIRED";
    public const string InvalidValue = "INVALID_VALUE";
    public const string InvalidFormat = "INVALID_FORMAT";
    public const string OutOfRange = "OUT_OF_RANGE";
    public const string TooShort = "TOO_SHORT";
    public const string TooLong = "TOO_LONG";
}
