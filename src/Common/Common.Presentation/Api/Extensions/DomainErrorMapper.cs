using System.Net;
using Common.Domain.Exceptions;
using System.Collections.Frozen;
using Common.Presentation.Api.Response;

namespace Common.Presentation.Api.Extensions;
public static class DomainErrorMapper
{
    private const HttpStatusCode DefaultStatus = HttpStatusCode.UnprocessableEntity;

    // Keys that must not be overridden by DomainException.Extensions (would produce duplicate JSON members).
    private static readonly FrozenSet<string> ReservedKeys = new[]
    {
        "type", "title", "status", "detail", "instance", "code", "traceId", "correlationId"
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// All codes that have an explicit mapping (used by the coverage unit test).
    /// </summary>
    public static IReadOnlyCollection<string> MappedCodes => StatusMap.Keys;
    public static int GetStatusCode(DomainException exception) => (int)StatusMap.GetValueOrDefault(exception.Code, DefaultStatus);
    public static ApiProblemDetails ToProblemDetails(DomainException exception, string? instance = null)
    {
        var problem = new ApiProblemDetails
        {
            Type = $"urn:domain-error:{exception.Code}",
            Title = Humanize(exception.Code),
            Status = GetStatusCode(exception),
            Detail = exception.Message,
            Instance = instance
        };

        problem.Extensions["code"] = exception.Code;

        foreach (var (key, value) in exception.Extensions)
        {
            if (!ReservedKeys.Contains(key))
                problem.Extensions[key] = value;
        }

        return problem;
    }
    public static string Humanize(string code)
    {
        var words = code.Split('_', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', words.Select(w => char.ToUpperInvariant(w[0]) + w[1..]));
    }

    public static readonly FrozenDictionary<string, HttpStatusCode> StatusMap = new Dictionary<string, HttpStatusCode>
    {
        // Existence / Identity
        [DomainExceptionCode.NotFound] = HttpStatusCode.NotFound,
        [DomainExceptionCode.AlreadyExists] = HttpStatusCode.Conflict,
        [DomainExceptionCode.Duplicate] = HttpStatusCode.Conflict,
        [DomainExceptionCode.Conflict] = HttpStatusCode.Conflict,
        [DomainExceptionCode.Gone] = HttpStatusCode.Gone,

        // Validation (shape / format of input → 400)
        [DomainExceptionCode.Required] = HttpStatusCode.BadRequest,
        [DomainExceptionCode.InvalidValue] = HttpStatusCode.BadRequest,
        [DomainExceptionCode.InvalidFormat] = HttpStatusCode.BadRequest,
        [DomainExceptionCode.InvalidType] = HttpStatusCode.BadRequest,
        [DomainExceptionCode.OutOfRange] = HttpStatusCode.BadRequest,
        [DomainExceptionCode.TooShort] = HttpStatusCode.BadRequest,
        [DomainExceptionCode.TooLong] = HttpStatusCode.BadRequest,
        [DomainExceptionCode.TooSmall] = HttpStatusCode.BadRequest,
        [DomainExceptionCode.TooLarge] = HttpStatusCode.BadRequest,
        [DomainExceptionCode.MustBePositive] = HttpStatusCode.BadRequest,
        [DomainExceptionCode.MustBeNegative] = HttpStatusCode.BadRequest,
        [DomainExceptionCode.MustBeZeroOrPositive] = HttpStatusCode.BadRequest,
        [DomainExceptionCode.MustNotBeEmpty] = HttpStatusCode.BadRequest,
        [DomainExceptionCode.MustBeEmpty] = HttpStatusCode.BadRequest,
        [DomainExceptionCode.NotUnique] = HttpStatusCode.Conflict,
        [DomainExceptionCode.InvalidLength] = HttpStatusCode.BadRequest,
        [DomainExceptionCode.InvalidPrecision] = HttpStatusCode.BadRequest,
        [DomainExceptionCode.InvalidEnumValue] = HttpStatusCode.BadRequest,

        // State / Lifecycle (business rules → 422)
        [DomainExceptionCode.InvalidOperation] = HttpStatusCode.UnprocessableEntity,
        [DomainExceptionCode.InvalidState] = HttpStatusCode.UnprocessableEntity,
        [DomainExceptionCode.InvalidStateTransition] = HttpStatusCode.UnprocessableEntity,
        [DomainExceptionCode.AlreadyProcessed] = HttpStatusCode.Conflict,
        [DomainExceptionCode.AlreadyCompleted] = HttpStatusCode.Conflict,
        [DomainExceptionCode.AlreadyCancelled] = HttpStatusCode.Conflict,
        [DomainExceptionCode.AlreadyDeleted] = HttpStatusCode.Conflict,
        [DomainExceptionCode.AlreadyActive] = HttpStatusCode.Conflict,
        [DomainExceptionCode.AlreadyInactive] = HttpStatusCode.Conflict,
        [DomainExceptionCode.NotReady] = HttpStatusCode.UnprocessableEntity,
        [DomainExceptionCode.NotActive] = HttpStatusCode.UnprocessableEntity,
        [DomainExceptionCode.Expired] = HttpStatusCode.UnprocessableEntity,
        [DomainExceptionCode.Locked] = HttpStatusCode.Locked,
        [DomainExceptionCode.Archived] = HttpStatusCode.UnprocessableEntity,
        [DomainExceptionCode.Immutable] = HttpStatusCode.UnprocessableEntity,

        // Relations / Integrity
        [DomainExceptionCode.ReferenceNotFound] = HttpStatusCode.UnprocessableEntity,
        [DomainExceptionCode.InvalidReference] = HttpStatusCode.UnprocessableEntity,
        [DomainExceptionCode.InUse] = HttpStatusCode.Conflict,
        [DomainExceptionCode.HasDependents] = HttpStatusCode.Conflict,
        [DomainExceptionCode.CircularReference] = HttpStatusCode.UnprocessableEntity,
        [DomainExceptionCode.InconsistentData] = HttpStatusCode.UnprocessableEntity,
        [DomainExceptionCode.IntegrityViolation] = HttpStatusCode.Conflict,
        [DomainExceptionCode.InvariantViolation] = HttpStatusCode.UnprocessableEntity,

        // Concurrency
        [DomainExceptionCode.ConcurrencyConflict] = HttpStatusCode.Conflict,
        [DomainExceptionCode.StaleVersion] = HttpStatusCode.Conflict,

        // Rules / Limits
        [DomainExceptionCode.NotAllowed] = HttpStatusCode.Forbidden,
        [DomainExceptionCode.LimitExceeded] = HttpStatusCode.UnprocessableEntity,
        [DomainExceptionCode.QuotaExceeded] = HttpStatusCode.TooManyRequests,
        [DomainExceptionCode.InsufficientBalance] = HttpStatusCode.UnprocessableEntity,
        [DomainExceptionCode.InsufficientQuantity] = HttpStatusCode.UnprocessableEntity,

        // Time / Period
        [DomainExceptionCode.InvalidDate] = HttpStatusCode.BadRequest,
        [DomainExceptionCode.InvalidTime] = HttpStatusCode.BadRequest,
        [DomainExceptionCode.InvalidPeriod] = HttpStatusCode.BadRequest,
        [DomainExceptionCode.InvalidDateRange] = HttpStatusCode.BadRequest,
        [DomainExceptionCode.StartAfterEnd] = HttpStatusCode.BadRequest,
        [DomainExceptionCode.Overlap] = HttpStatusCode.Conflict,
        [DomainExceptionCode.Gap] = HttpStatusCode.UnprocessableEntity,
        [DomainExceptionCode.InPast] = HttpStatusCode.BadRequest,
        [DomainExceptionCode.InFuture] = HttpStatusCode.BadRequest,

        // Data / Format
        [DomainExceptionCode.InvalidCurrency] = HttpStatusCode.BadRequest,
        [DomainExceptionCode.CurrencyMismatch] = HttpStatusCode.UnprocessableEntity,
        [DomainExceptionCode.UnitMismatch] = HttpStatusCode.UnprocessableEntity,
        [DomainExceptionCode.UnsupportedValue] = HttpStatusCode.BadRequest,
        [DomainExceptionCode.NotSupported] = HttpStatusCode.UnprocessableEntity,
        [DomainExceptionCode.NotImplemented] = HttpStatusCode.NotImplemented,
        [DomainExceptionCode.Mismatch] = HttpStatusCode.UnprocessableEntity,

        // Generic
        [DomainExceptionCode.Unexpected] = HttpStatusCode.InternalServerError,
        [DomainExceptionCode.Unknown] = HttpStatusCode.InternalServerError,
    }.ToFrozenDictionary();
}