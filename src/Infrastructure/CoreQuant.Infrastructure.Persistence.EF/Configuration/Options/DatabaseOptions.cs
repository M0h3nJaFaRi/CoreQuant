namespace CoreQuant.Infrastructure.Persistence.EF.Configuration.Options;

/// <summary>
/// Named MySQL endpoints under <c>Infrastructure:Databases</c> (one entry per EF DbContext).
/// </summary>
public sealed class DatabaseOptions : Dictionary<string, DatabaseEndpointOptions>
{
    /// <summary>
    /// Configuration section name under the host tree.
    /// </summary>
    public const string SectionName = "Infrastructure:Databases";
}
