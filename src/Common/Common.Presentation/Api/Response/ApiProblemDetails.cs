using System.Text.Json.Serialization;

namespace Common.Presentation.Api.Response;
public sealed class ApiProblemDetails
{
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("status")]
    public int Status { get; init; }

    [JsonPropertyName("detail")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Detail { get; init; }

    [JsonPropertyName("instance")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Instance { get; init; }

    /// <summary>
    /// Extra members (code, traceId, ...) are flattened into the root JSON object.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object?> Extensions { get; set; } = [];
}