namespace Framework.Security.Hashing;

/// <summary>
/// Cryptographic hashing helpers (SHA / HMAC). Not for password hashing or identity providers.
/// </summary>
public interface IHashService
{
    /// <summary>
    /// Computes a SHA-256 hash of <paramref name="data"/> and returns a lowercase hex string.
    /// </summary>
    string ComputeSha256(ReadOnlySpan<byte> data);

    /// <summary>
    /// Computes a SHA-256 hash of UTF-8 encoded <paramref name="text"/> and returns a lowercase hex string.
    /// </summary>
    string ComputeSha256(string text);

    /// <summary>
    /// Computes an HMAC-SHA256 of <paramref name="data"/> using <paramref name="key"/> and returns a lowercase hex string.
    /// </summary>
    string ComputeHmacSha256(ReadOnlySpan<byte> key, ReadOnlySpan<byte> data);

    /// <summary>
    /// Computes an HMAC-SHA256 of UTF-8 encoded <paramref name="text"/> using UTF-8 encoded <paramref name="key"/>
    /// and returns a lowercase hex string.
    /// </summary>
    string ComputeHmacSha256(string key, string text);
}
