namespace Common.Presentation.Api.Configuration.Options;
public sealed class RequestLoggingOptions
{
    public const string SectionName = "RequestLogging";

    /// <summary>
    /// Successful requests to these paths are logged at Debug instead of Information.
    /// </summary>
    public string[] QuietPaths { get; set; } = ["/health"];
}