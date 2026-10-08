using Microsoft.AspNetCore.Authentication;

namespace CoreQuant.Infrastructure.Acl.Authentication;

/// <summary>
/// Options for the development <c>DevUser</c> authentication scheme.
/// Local/dev stand-in until a real JWT/OIDC authority is wired — never enabled implicitly.
/// </summary>
public sealed class DevUserAuthenticationOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// When false, the handler ignores the dev header and every request stays anonymous.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Request header carrying the acting user id (snowflake).
    /// </summary>
    public string HeaderName { get; set; } = "X-User-Id";
}
