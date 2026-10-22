namespace Common.Presentation.Api.Configuration.Options;
public sealed class MaintenanceOptions
{
    public const string SectionName = "Maintenance";

    public bool Enabled { get; set; }
    public string Message { get; set; } = "Service is temporarily unavailable.";
    public int RetryAfterSeconds { get; set; } = 300;

    // Note: the configuration binder APPENDS configured items to this default (it doesn't replace it),
    // so "/health" always stays excluded. Duplicates are harmless.
    public string[] ExcludedPaths { get; set; } = ["/health"];
}