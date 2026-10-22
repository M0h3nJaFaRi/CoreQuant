namespace Common.Presentation.Api.Configuration.Options;
public sealed class ApiKeyClientOptions
{
    /// <summary>
    /// One or more active keys. Multiple keys per client allow zero-downtime rotation.
    /// </summary>
    public string[] Keys { get; set; } = [];

    /// <summary>
    /// Quick way to disable a client without deleting its config.
    /// </summary>
    public bool Enabled { get; set; }
}