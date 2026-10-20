using System.Security.Claims;
using Common.Application.Authorization.Abstraction;
using Microsoft.AspNetCore.Http;

namespace CoreQuant.Infrastructure.Acl.CurrentUser;

/// <summary>
/// Reads the acting user id from the ASP.NET Core <see cref="ClaimsPrincipal"/>.
/// Null when the request is anonymous — authentication decides identity, this type only surfaces it.
/// </summary>
internal sealed class HttpCurrentUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserAccessor
{
    /// <inheritdoc />
    public Guid? UserId
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            
            if (user?.Identity?.IsAuthenticated != true)
                return null;

            var raw = user.FindFirstValue(ClaimTypes.NameIdentifier);
            
            return Guid.TryParse(raw, out var userId) ? userId : null;
        }
    }
}