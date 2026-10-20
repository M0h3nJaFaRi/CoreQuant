namespace Common.Presentation.Api.Configuration.Options;

/// <summary>
/// OpenAPI / Swagger UI settings bound from <c>Framework:Swagger</c>.
/// </summary>
public sealed class SwaggerOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Framework:Swagger";

    /// <summary>
    /// When false, Swagger middleware and UI are not registered.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// OpenAPI document name (e.g. <c>v1</c>).
    /// </summary>
    public string DocumentName { get; set; } = "v1";

    /// <summary>
    /// UI route prefix (e.g. <c>swagger</c> → <c>/swagger</c>).
    /// </summary>
    public string RoutePrefix { get; set; } = "swagger";

    /// <summary>
    /// Optional API title override. Defaults to <c>ApplicationName</c>.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Optional API description shown in Swagger UI.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Documents the development <c>X-User-Id</c> header (CoreQuant.Api + DevUser scheme).
    /// </summary>
    public bool IncludeDevUserHeader { get; set; }

    /// <summary>
    /// Documents the collector <c>X-Api-Key</c> header for tick and collector routes.
    /// </summary>
    public bool IncludeIngestionApiKey { get; set; }
}
