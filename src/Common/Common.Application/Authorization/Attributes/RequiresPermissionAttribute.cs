namespace Common.Application.Authorization.Attributes;

/// <summary>
/// Declares the permission(s) required to invoke a CQRS request. Checked at the
/// type level only — not a substitute for resource-level checks inside the handler.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class RequiresPermissionAttribute(params string[] permissions) : Attribute
{
    /// <summary>The permission codes required (e.g. "payment.read").</summary>
    public IReadOnlyCollection<string> Permissions { get; } = permissions;

    /// <summary>When true, the caller needs ANY one of <see cref="Permissions"/>; otherwise ALL of them. Default: all.</summary>
    public bool RequireAny { get; init; }
}