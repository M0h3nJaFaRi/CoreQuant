namespace Common.Presentation.Api.Configuration.Options;

public sealed class ApiKeyOptions
{
    public const string SectionName = "Middlewares:ApiKey";   // matches your JSON

    public string HeaderName { get; set; } = default!;
    public string[] ExcludedPaths { get; set; } = [];

    // Case-insensitive so env vars like Middlewares__ApiKey__Clients__Mobile-App__... still match.
    public Dictionary<string, ApiKeyClientOptions> Clients { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}