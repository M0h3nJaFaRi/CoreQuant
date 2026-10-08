using System.Text.Json;

namespace Framework.Serialization.Json;

/// <summary>
/// Technology-agnostic JSON serialize/deserialize contract.
/// </summary>
public interface IJsonSerializer
{
    /// <summary>
    /// Serializes <paramref name="value"/> to a JSON string.
    /// </summary>
    string Serialize<T>(T value, JsonSerializerOptions? options = null);

    /// <summary>
    /// Serializes <paramref name="value"/> using its runtime type.
    /// </summary>
    string Serialize(object value, JsonSerializerOptions? options = null);

    /// <summary>
    /// Deserializes JSON into <typeparamref name="T"/>.
    /// </summary>
    T? Deserialize<T>(string json, JsonSerializerOptions? options = null);

    /// <summary>
    /// Serializes <paramref name="value"/> to UTF-8 JSON bytes.
    /// </summary>
    byte[] SerializeToUtf8Bytes<T>(T value, JsonSerializerOptions? options = null);

    /// <summary>
    /// Deserializes UTF-8 JSON bytes into <typeparamref name="T"/>.
    /// </summary>
    T? Deserialize<T>(ReadOnlySpan<byte> utf8Json, JsonSerializerOptions? options = null);
}
