using System.Text.Json;
using System.Text.Json.Serialization;

namespace Framework.Serialization.Json;

/// <summary>
/// Default <see cref="IJsonSerializer"/> using <see cref="JsonSerializer"/>.
/// </summary>
internal sealed class SystemTextJsonSerializer : IJsonSerializer
{
    private static readonly JsonSerializerOptions DefaultOptions = CreateDefaultOptions();

    /// <inheritdoc />
    public string Serialize<T>(T value, JsonSerializerOptions? options = null)
        => JsonSerializer.Serialize(value, options ?? DefaultOptions);

    /// <inheritdoc />
    public string Serialize(object value, JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(value);
        return JsonSerializer.Serialize(value, value.GetType(), options ?? DefaultOptions);
    }

    /// <inheritdoc />
    public T? Deserialize<T>(string json, JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(json);
        return JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions);
    }

    /// <inheritdoc />
    public byte[] SerializeToUtf8Bytes<T>(T value, JsonSerializerOptions? options = null)
        => JsonSerializer.SerializeToUtf8Bytes(value, options ?? DefaultOptions);

    /// <inheritdoc />
    public T? Deserialize<T>(ReadOnlySpan<byte> utf8Json, JsonSerializerOptions? options = null)
        => JsonSerializer.Deserialize<T>(utf8Json, options ?? DefaultOptions);

    private static JsonSerializerOptions CreateDefaultOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
            ReferenceHandler = ReferenceHandler.Preserve,
            ReadCommentHandling = JsonCommentHandling.Disallow,
        };
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return options;
    }
}
