using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoreQuant.Infrastructure.Acl.Authentication;

/// <summary>
/// Development authentication handler building a <see cref="ClaimsPrincipal"/> from a user-id header.
/// Active only when <see cref="DevUserAuthenticationOptions.Enabled"/> is true; otherwise every
/// request stays anonymous (fail-closed when the flag is off, e.g. production).
/// </summary>
internal sealed class DevUserAuthenticationHandler : AuthenticationHandler<DevUserAuthenticationOptions>
{
    public DevUserAuthenticationHandler(
        IOptionsMonitor<DevUserAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    /// <inheritdoc />
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Options.Enabled)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var header = Request.Headers[Options.HeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(header) || !long.TryParse(header.Trim(), out var userId) || userId <= 0)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("auth_scheme", Scheme.Name),
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
