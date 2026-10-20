namespace Common.Application.Authorization.Contracts;

/// <summary>Result of an authorization check, with an optional bounded reason code for metrics/logging.</summary>
public readonly record struct AuthorizationResult(bool Allowed, string? Reason = null)
{
    public static AuthorizationResult Allow() => new(true);
    public static AuthorizationResult Deny(string reason) => new(false, reason);
}