namespace Framework.Security.Configuration.Options;

/// <summary>
/// AES key material for <c>Framework.Security</c> encryption services.
/// Values are Base64-encoded raw key / IV bytes.
/// </summary>
public sealed class AesEncryptionOptions
{
    /// <summary>
    /// Base64-encoded AES key (16, 24, or 32 bytes when decoded).
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Base64-encoded AES IV (16 bytes when decoded).
    /// </summary>
    public string IV { get; set; } = string.Empty;

    /// <summary>
    /// True when both key and IV are present.
    /// </summary>
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Key) && !string.IsNullOrWhiteSpace(IV);
}
