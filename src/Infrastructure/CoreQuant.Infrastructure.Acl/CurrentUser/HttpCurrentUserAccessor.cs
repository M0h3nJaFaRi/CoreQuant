using System.Security.Claims;
using Common.Application.Authorization;
using Microsoft.AspNetCore.Http;

namespace CoreQuant.Infrastructure.Acl.CurrentUser;

/// <summary>
/// Reads the acting user id from the ASP.NET Core <see cref="ClaimsPrincipal"/>.
/// Null when the request is anonymous — authentication decides identity, this type only surfaces it.
/// </summary>
internal sealed class HttpCurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor);
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public long? UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var raw = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return long.TryParse(raw, out var userId) && userId > 0 ? userId : null;
        }
    }
}
