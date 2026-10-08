namespace Framework.Security.Configuration.Options;

/// <summary>
/// Framework encryption settings bound from <c>Framework:Encryption</c>.
/// </summary>
public sealed class EncryptionOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Framework:Encryption";

    /// <summary>
    /// RSA public / private PEM keys.
    /// </summary>
    public RsaEncryptionOptions RSA { get; set; } = new();

    /// <summary>
    /// AES key and IV (Base64).
    /// </summary>
    public AesEncryptionOptions AES { get; set; } = new();
}
