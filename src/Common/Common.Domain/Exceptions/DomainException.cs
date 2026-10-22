using System.Collections.Frozen;
using System.Globalization;
using System.Net;
using System.Text.Json.Serialization;

namespace Common.Domain.Exceptions;

/// <summary>
/// Thrown when a domain invariant is violated.
/// </summary>

// Domain layer. No reference to HTTP, ProblemDetails or status codes.
// Status/ProblemDetails mapping lives in the Api layer (DomainErrorMapper).
public sealed class DomainException : Exception
{
    private readonly Dictionary<string, object?> _extensions = [];

    private DomainException(string code, string message) : base(message)
    {
        Code = code.Trim();
    }

    /// <summary>
    /// Machine-readable error code (e.g. <c>not_found</c>).
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Extra data for consumers (e.g. field, key, limit).
    /// </summary>
    public IReadOnlyDictionary<string, object?> Extensions => _extensions;

    public DomainException WithExtension(string key, object? value)
    {
        _extensions[key] = value;
        return this;
    }

    // ───────────── Existence / Identity ─────────────
    public static DomainException NotFound(string entity, object? key = null) =>
        Create(DomainExceptionCode.NotFound, $"{entity}{Quote(key)} was not found.");

    public static DomainException AlreadyExists(string entity, object? key = null) =>
        Create(DomainExceptionCode.AlreadyExists, $"{entity}{Quote(key)} already exists.");

    public static DomainException Duplicate(string entity, object? key = null) =>
        Create(DomainExceptionCode.Duplicate, $"Duplicate {entity}{Quote(key)}.");

    public static DomainException Conflict(string message) =>
        Create(DomainExceptionCode.Conflict, message);

    public static DomainException Gone(string entity, object? key = null) =>
        Create(DomainExceptionCode.Gone, $"{entity}{Quote(key)} is no longer available.");

    // ───────────── Validation ─────────────
    public static DomainException Required(string field) =>
        Create(DomainExceptionCode.Required, $"{field} is required.");

    public static DomainException InvalidValue(string field, object? value = null) =>
        Create(DomainExceptionCode.InvalidValue, $"{field} has an invalid value{Quote(value)}.");

    public static DomainException InvalidFormat(string field, string? expectedFormat = null) =>
        Create(DomainExceptionCode.InvalidFormat, expectedFormat is null ? $"{field} has an invalid format." : $"{field} has an invalid format. Expected: {expectedFormat}.");

    public static DomainException InvalidType(string field, string? expectedType = null) =>
        Create(DomainExceptionCode.InvalidType, expectedType is null ? $"{field} has an invalid type." : $"{field} has an invalid type. Expected: {expectedType}.");

    public static DomainException OutOfRange(string field, object? min = null, object? max = null) =>
        Create(DomainExceptionCode.OutOfRange,
            (min, max) switch
            {
                (null, null) => $"{field} is out of range.",
                (not null, null) => $"{field} is out of range (min: {F(min)}).",
                (null, not null) => $"{field} is out of range (max: {F(max)}).",
                _ => $"{field} is out of range ({F(min)}..{F(max)})."
            });

    public static DomainException TooShort(string field, int minLength) =>
        Create(DomainExceptionCode.TooShort, $"{field} is too short (min length: {F(minLength)}).");

    public static DomainException TooLong(string field, int maxLength) =>
        Create(DomainExceptionCode.TooLong, $"{field} is too long (max length: {F(maxLength)}).");

    public static DomainException TooSmall(string field, object min) =>
        Create(DomainExceptionCode.TooSmall, $"{field} is too small (min: {F(min)}).");

    public static DomainException TooLarge(string field, object max) =>
        Create(DomainExceptionCode.TooLarge, $"{field} is too large (max: {F(max)}).");

    public static DomainException MustBePositive(string field) =>
        Create(DomainExceptionCode.MustBePositive, $"{field} must be positive.");

    public static DomainException MustBeNegative(string field) =>
        Create(DomainExceptionCode.MustBeNegative, $"{field} must be negative.");

    public static DomainException MustBeZeroOrPositive(string field) =>
        Create(DomainExceptionCode.MustBeZeroOrPositive, $"{field} must be zero or positive.");

    public static DomainException MustNotBeEmpty(string field) =>
        Create(DomainExceptionCode.MustNotBeEmpty, $"{field} must not be empty.");

    public static DomainException MustBeEmpty(string field) =>
        Create(DomainExceptionCode.MustBeEmpty, $"{field} must be empty.");

    public static DomainException NotUnique(string field) =>
        Create(DomainExceptionCode.NotUnique, $"{field} must be unique.");

    public static DomainException InvalidLength(string field, int expectedLength) =>
        Create(DomainExceptionCode.InvalidLength, $"{field} must have a length of {F(expectedLength)}.");

    public static DomainException InvalidPrecision(string field, int maxDecimals) =>
        Create(DomainExceptionCode.InvalidPrecision, $"{field} has too many decimal places (max: {F(maxDecimals)}).");

    public static DomainException InvalidEnumValue(string field, object? value = null) =>
        Create(DomainExceptionCode.InvalidEnumValue, $"{field} has an invalid enum value{Quote(value)}.");

    // ───────────── State / Lifecycle ─────────────
    public static DomainException InvalidOperation(string message) =>
        Create(DomainExceptionCode.InvalidOperation, message);

    public static DomainException InvalidState(string entity, object? current = null, object? expected = null)
    {
        var details = (current, expected) switch
        {
            (null, null) => string.Empty,
            (not null, null) => $" (current: {F(current)})",
            (null, not null) => $" (expected: {F(expected)})",
            _ => $" (current: {F(current)}, expected: {F(expected)})"
        };

        return Create(DomainExceptionCode.InvalidState, $"{entity} is in an invalid state{details}.");
    }

    public static DomainException InvalidStateTransition(string entity, object from, object to) =>
        Create(DomainExceptionCode.InvalidStateTransition, $"{entity} cannot transition from '{F(from)}' to '{F(to)}'.");

    public static DomainException AlreadyProcessed(string entity, object? key = null) =>
        Create(DomainExceptionCode.AlreadyProcessed, $"{entity}{Quote(key)} has already been processed.");

    public static DomainException AlreadyCompleted(string entity, object? key = null) =>
        Create(DomainExceptionCode.AlreadyCompleted, $"{entity}{Quote(key)} is already completed.");

    public static DomainException AlreadyCancelled(string entity, object? key = null) =>
        Create(DomainExceptionCode.AlreadyCancelled, $"{entity}{Quote(key)} is already cancelled.");

    public static DomainException AlreadyDeleted(string entity, object? key = null) =>
        Create(DomainExceptionCode.AlreadyDeleted, $"{entity}{Quote(key)} is already deleted.");

    public static DomainException AlreadyActive(string entity, object? key = null) =>
        Create(DomainExceptionCode.AlreadyActive, $"{entity}{Quote(key)} is already active.");

    public static DomainException AlreadyInactive(string entity, object? key = null) =>
        Create(DomainExceptionCode.AlreadyInactive, $"{entity}{Quote(key)} is already inactive.");

    public static DomainException NotReady(string entity, object? key = null) =>
        Create(DomainExceptionCode.NotReady, $"{entity}{Quote(key)} is not ready.");

    public static DomainException NotActive(string entity, object? key = null) =>
        Create(DomainExceptionCode.NotActive, $"{entity}{Quote(key)} is not active.");

    public static DomainException Expired(string entity, object? key = null) =>
        Create(DomainExceptionCode.Expired, $"{entity}{Quote(key)} has expired.");

    public static DomainException Locked(string entity, object? key = null) =>
        Create(DomainExceptionCode.Locked, $"{entity}{Quote(key)} is locked.");

    public static DomainException Archived(string entity, object? key = null) =>
        Create(DomainExceptionCode.Archived, $"{entity}{Quote(key)} is archived.");

    public static DomainException Immutable(string entity, string? member = null) =>
        Create(DomainExceptionCode.Immutable, member is null ? $"{entity} is immutable." : $"{entity}.{member} is immutable and cannot be changed.");

    // ───────────── Relations / Integrity ─────────────
    public static DomainException ReferenceNotFound(string reference, object? key = null) =>
        Create(DomainExceptionCode.ReferenceNotFound, $"Referenced {reference}{Quote(key)} was not found.");

    public static DomainException InvalidReference(string reference, object? key = null) =>
        Create(DomainExceptionCode.InvalidReference, $"Invalid reference to {reference}{Quote(key)}.");

    public static DomainException InUse(string entity, object? key = null) =>
        Create(DomainExceptionCode.InUse, $"{entity}{Quote(key)} is in use.");

    public static DomainException HasDependents(string entity, string? dependents = null) =>
        Create(DomainExceptionCode.HasDependents, dependents is null ? $"{entity} has dependent records." : $"{entity} has dependent {dependents}.");

    public static DomainException CircularReference(string entity) =>
        Create(DomainExceptionCode.CircularReference, $"Circular reference detected in {entity}.");

    public static DomainException InconsistentData(string message) =>
        Create(DomainExceptionCode.InconsistentData, message);

    public static DomainException IntegrityViolation(string message) =>
        Create(DomainExceptionCode.IntegrityViolation, message);

    public static DomainException InvariantViolation(string message) =>
        Create(DomainExceptionCode.InvariantViolation, message);

    // ───────────── Concurrency ─────────────
    public static DomainException ConcurrencyConflict(string entity, object? key = null) =>
        Create(DomainExceptionCode.ConcurrencyConflict,
            $"{entity}{Quote(key)} was modified by another process.");

    public static DomainException StaleVersion(string entity, object? key = null) =>
        Create(DomainExceptionCode.StaleVersion, $"{entity}{Quote(key)} version is stale.");

    // ───────────── Rules / Limits ─────────────
    // Authentication / authorization (Unauthorized, Forbidden) are intentionally NOT here:
    // they belong to the Api/Application layer (ASP.NET Core auth pipeline).
    public static DomainException NotAllowed(string action) =>
        Create(DomainExceptionCode.NotAllowed, $"{action} is not allowed.");

    public static DomainException LimitExceeded(string subject, object? limit = null) =>
        Create(DomainExceptionCode.LimitExceeded, limit is null ? $"{subject} limit exceeded." : $"{subject} limit exceeded (limit: {F(limit)}).");

    public static DomainException QuotaExceeded(string subject, object? quota = null) =>
        Create(DomainExceptionCode.QuotaExceeded, quota is null ? $"{subject} quota exceeded." : $"{subject} quota exceeded (quota: {F(quota)}).");

    public static DomainException InsufficientBalance(object? required = null, object? available = null) =>
        Create(DomainExceptionCode.InsufficientBalance, required is null || available is null ? "Insufficient balance." : $"Insufficient balance (required: {F(required)}, available: {F(available)}).");

    public static DomainException InsufficientQuantity(string item, object? required = null, object? available = null) =>
        Create(DomainExceptionCode.InsufficientQuantity, required is null || available is null ? $"Insufficient quantity of {item}." : $"Insufficient quantity of {item} (required: {F(required)}, available: {F(available)}).");

    // ───────────── Time / Period ─────────────
    public static DomainException InvalidDate(string field, object? value = null) =>
        Create(DomainExceptionCode.InvalidDate, $"{field} is not a valid date{Quote(value)}.");

    public static DomainException InvalidTime(string field, object? value = null) =>
        Create(DomainExceptionCode.InvalidTime, $"{field} is not a valid time{Quote(value)}.");

    public static DomainException InvalidPeriod(string field = "Period") =>
        Create(DomainExceptionCode.InvalidPeriod, $"{field} is not valid.");

    public static DomainException InvalidDateRange(string field = "Date range") =>
        Create(DomainExceptionCode.InvalidDateRange, $"{field} is not valid.");

    public static DomainException StartAfterEnd(string startField = "Start", string endField = "End") =>
        Create(DomainExceptionCode.StartAfterEnd, $"{startField} must not be after {endField}.");

    public static DomainException Overlap(string entity, string? detail = null) =>
        Create(DomainExceptionCode.Overlap, detail is null ? $"{entity} overlaps with an existing one." : $"{entity} overlaps: {detail}.");

    public static DomainException Gap(string entity, string? detail = null) =>
        Create(DomainExceptionCode.Gap, detail is null ? $"A gap was detected in {entity}." : $"A gap was detected in {entity}: {detail}.");

    public static DomainException InPast(string field) =>
        Create(DomainExceptionCode.InPast, $"{field} must not be in the past.");

    public static DomainException InFuture(string field) =>
        Create(DomainExceptionCode.InFuture, $"{field} must not be in the future.");

    // ───────────── Data / Format ─────────────
    public static DomainException InvalidCurrency(object? value = null) =>
        Create(DomainExceptionCode.InvalidCurrency, $"Invalid currency{Quote(value)}.");

    public static DomainException CurrencyMismatch(object expected, object actual) =>
        Create(DomainExceptionCode.CurrencyMismatch,
            $"Currency mismatch (expected: {F(expected)}, actual: {F(actual)}).");

    public static DomainException UnitMismatch(object expected, object actual) =>
        Create(DomainExceptionCode.UnitMismatch, $"Unit mismatch (expected: {F(expected)}, actual: {F(actual)}).");

    public static DomainException UnsupportedValue(string field, object? value = null) =>
        Create(DomainExceptionCode.UnsupportedValue, $"{field} has an unsupported value{Quote(value)}.");

    public static DomainException NotSupported(string feature) =>
        Create(DomainExceptionCode.NotSupported, $"{feature} is not supported.");

    public static DomainException NotImplemented(string feature) =>
        Create(DomainExceptionCode.NotImplemented, $"{feature} is not implemented.");

    public static DomainException Mismatch(string what, object? expected = null, object? actual = null) =>
        Create(DomainExceptionCode.Mismatch, expected is null || actual is null ? $"{what} mismatch." : $"{what} mismatch (expected: {F(expected)}, actual: {F(actual)}).");

    // ───────────── Generic ─────────────
    public static DomainException Unexpected(string message = "An unexpected error occurred.") => Create(DomainExceptionCode.Unexpected, message);
    public static DomainException Unknown(string message = "An unknown error occurred.") => Create(DomainExceptionCode.Unknown, message);

    /// <summary>
    /// Creates a domain exception with an explicit code (used by invariant guards).
    /// </summary>
    public static DomainException Guard(string message, string code) => Create(code, message);

    // ───────────── Helpers ─────────────
    private static DomainException Create(string code, string message) => new(code, message);

    // Culture-invariant formatting so messages don't depend on the host's culture (1.5 vs 1,5).
    private static string F(object? value) => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;

    private static string Quote(object? value) => value is null ? string.Empty : $" '{F(value)}'";
}


public sealed class ProblemDetails
{
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("status")]
    public int Status { get; init; }

    [JsonPropertyName("detail")]
    public string? Detail { get; init; }

    [JsonPropertyName("instance")]
    public string? Instance { get; init; }


    /// <summary>Extra members (code, field, ...) are flattened into the root JSON object.</summary>
    [JsonExtensionData]
    public Dictionary<string, object?> Extensions { get; set; } = [];
}