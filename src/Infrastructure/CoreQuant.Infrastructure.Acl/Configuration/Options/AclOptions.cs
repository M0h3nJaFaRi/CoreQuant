namespace CoreQuant.Infrastructure.Acl.Configuration.Options;

/// <summary>
/// Access-control settings (auth schemes, future external/microservice clients).
/// Bound from <c>Infrastructure:Acl</c>.
/// </summary>
public sealed class AclOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Infrastructure:Acl";

    /// <summary>
    /// When true, the development <c>DevUser</c> authentication scheme is active.
    /// </summary>
    public bool DevAuthEnabled { get; set; }
}
