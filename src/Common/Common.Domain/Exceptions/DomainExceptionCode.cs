namespace Common.Domain.Exceptions;
public static class DomainExceptionCode
{
    // ───────────── Existence / Identity ─────────────
    public const string NotFound = "not_found";
    public const string AlreadyExists = "already_exists";
    public const string Duplicate = "duplicate";
    public const string Conflict = "conflict";
    public const string Gone = "gone";

    // ───────────── Validation ─────────────
    public const string Required = "required";
    public const string InvalidValue = "invalid_value";
    public const string InvalidFormat = "invalid_format";
    public const string InvalidType = "invalid_type";
    public const string OutOfRange = "out_of_range";
    public const string TooShort = "too_short";
    public const string TooLong = "too_long";
    public const string TooSmall = "too_small";
    public const string TooLarge = "too_large";
    public const string MustBePositive = "must_be_positive";
    public const string MustBeNegative = "must_be_negative";
    public const string MustBeZeroOrPositive = "must_be_zero_or_positive";
    public const string MustNotBeEmpty = "must_not_be_empty";
    public const string MustBeEmpty = "must_be_empty";
    public const string NotUnique = "not_unique";
    public const string InvalidLength = "invalid_length";
    public const string InvalidPrecision = "invalid_precision";
    public const string InvalidEnumValue = "invalid_enum_value";

    // ───────────── State / Lifecycle ─────────────
    public const string InvalidOperation = "invalid_operation";
    public const string InvalidState = "invalid_state";
    public const string InvalidStateTransition = "invalid_state_transition";
    public const string AlreadyProcessed = "already_processed";
    public const string AlreadyCompleted = "already_completed";
    public const string AlreadyCancelled = "already_cancelled";
    public const string AlreadyDeleted = "already_deleted";
    public const string AlreadyActive = "already_active";
    public const string AlreadyInactive = "already_inactive";
    public const string NotReady = "not_ready";
    public const string NotActive = "not_active";
    public const string Expired = "expired";
    public const string Locked = "locked";
    public const string Archived = "archived";
    public const string Immutable = "immutable";

    // ───────────── Relations / Integrity ─────────────
    public const string ReferenceNotFound = "reference_not_found";
    public const string InvalidReference = "invalid_reference";
    public const string InUse = "in_use";
    public const string HasDependents = "has_dependents";
    public const string CircularReference = "circular_reference";
    public const string InconsistentData = "inconsistent_data";
    public const string IntegrityViolation = "integrity_violation";
    public const string InvariantViolation = "invariant_violation";

    // ───────────── Concurrency ─────────────
    public const string ConcurrencyConflict = "concurrency_conflict";
    public const string StaleVersion = "stale_version";

    // ───────────── Authorization / Limits ─────────────
    public const string Forbidden = "forbidden";
    public const string Unauthorized = "unauthorized";
    public const string NotAllowed = "not_allowed";
    public const string LimitExceeded = "limit_exceeded";
    public const string QuotaExceeded = "quota_exceeded";
    public const string InsufficientBalance = "insufficient_balance";
    public const string InsufficientQuantity = "insufficient_quantity";

    // ───────────── Time / Period ─────────────
    public const string InvalidDate = "invalid_date";
    public const string InvalidTime = "invalid_time";
    public const string InvalidPeriod = "invalid_period";
    public const string InvalidDateRange = "invalid_date_range";
    public const string StartAfterEnd = "start_after_end";
    public const string Overlap = "overlap";
    public const string Gap = "gap";
    public const string InPast = "in_past";
    public const string InFuture = "in_future";

    // ───────────── Data / Format ─────────────
    public const string InvalidCurrency = "invalid_currency";
    public const string CurrencyMismatch = "currency_mismatch";
    public const string UnitMismatch = "unit_mismatch";
    public const string UnsupportedValue = "unsupported_value";
    public const string NotSupported = "not_supported";
    public const string NotImplemented = "not_implemented";
    public const string Mismatch = "mismatch";

    // ───────────── Generic ─────────────
    public const string Unexpected = "unexpected";
    public const string Unknown = "unknown";
}