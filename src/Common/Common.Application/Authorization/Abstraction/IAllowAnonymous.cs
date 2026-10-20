using Common.Application.Authorization.Attributes;

namespace Common.Application.Authorization.Abstraction;

/// <summary>
/// Marks a request as explicitly requiring no authorization check.
/// Must be used deliberately — the startup validator fails the build/boot
/// if a request has neither this marker, a <see cref="RequiresPermissionAttribute"/>,
/// nor a registered <see cref="IAuthorizationPolicy{TRequest}"/>.
/// </summary>
public interface IAllowAnonymous { }
