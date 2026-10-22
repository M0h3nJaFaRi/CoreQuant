namespace Common.Presentation.Api.Middlewares.TimeoutHandling;
public static class RequestTimeoutPolicies
{
    public const string Fast = nameof(Fast);
    public const string Standard = nameof(Standard);
    public const string LongRunning = nameof(LongRunning);
    public const string ThirdParty = nameof(ThirdParty);
}