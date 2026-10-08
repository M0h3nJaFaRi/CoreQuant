using System.Security.Cryptography;
using System.Text;

namespace Framework.Security.Hashing;

/// <summary>
/// Default <see cref="IHashService"/> using BCL cryptography APIs.
/// </summary>
internal sealed class HashService : IHashService
{
    /// <inheritdoc />
    public string ComputeSha256(ReadOnlySpan<byte> data)
        => Convert.ToHexString(SHA256.HashData(data)).ToLowerInvariant();

    /// <inheritdoc />
    public string ComputeSha256(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        return ComputeSha256(Encoding.UTF8.GetBytes(text));
    }

    /// <inheritdoc />
    public string ComputeHmacSha256(ReadOnlySpan<byte> key, ReadOnlySpan<byte> data)
        => Convert.ToHexString(HMACSHA256.HashData(key, data)).ToLowerInvariant();

    /// <inheritdoc />
    public string ComputeHmacSha256(string key, string text)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(text);
        return ComputeHmacSha256(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(text));
    }
}
