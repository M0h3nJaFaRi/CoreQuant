namespace Common.Application.Cqrs.Abstractions;

/// <summary>
/// Optional overrides for CQRS rate limiting on a specific request type.
/// </summary>
public interface IRateLimitedRequest
{
    /// <summary>
    /// Optional partition key. Null uses the default derived key (user + request type).
    /// </summary>
    string? RateLimitKey { get; }

    /// <summary>
    /// Optional permit limit override. Null uses configured defaults.
    /// </summary>
    int? PermitLimit { get; }

    /// <summary>
    /// Optional window override. Null uses configured defaults.
    /// </summary>
    TimeSpan? Window { get; }
}
