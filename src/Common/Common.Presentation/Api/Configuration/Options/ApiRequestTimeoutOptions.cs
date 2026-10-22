namespace Common.Presentation.Api.Configuration.Options;
public sealed class ApiRequestTimeoutOptions
{
    public const string SectionName = "RequestTimeouts";

    public int DefaultSeconds { get; init; } = 15;
    public int FastSeconds { get; init; } = 5;
    public int StandardSeconds { get; init; } = 15;
    public int LongRunningSeconds { get; init; } = 45;
    public int ThirdPartySeconds { get; init; } = 60;
}