namespace CoreQuant.Infrastructure.Persistence.EF.Configuration.Options;

/// <summary>
/// MySQL connection settings for the single MVP database (no read replica).
/// </summary>
public sealed record DatabaseOptions
{
    /// <summary>
    /// Configuration section name under the host tree.
    /// </summary>
    public const string SectionName = "Infrastructure:Databases";
    
    /// <summary>
    /// Configuration section name under the host tree.
    /// </summary>
    public IReadOnlyDictionary<string, DatabaseEndpointOptions> DataBases { get; init; } = default!;
}