namespace Framework.Security.Configuration.Options;

/// <summary>
/// RSA key material for <c>Framework.Security</c> encryption services.
/// Values are PEM-encoded public / private keys.
/// </summary>
public sealed class RsaEncryptionOptions
{
    /// <summary>
    /// PEM-encoded RSA public key (SubjectPublicKeyInfo).
    /// </summary>
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>
    /// PEM-encoded RSA private key (PKCS#8).
    /// </summary>
    public string PrivateKey { get; set; } = string.Empty;

    /// <summary>
    /// True when a public key is present (encrypt).
    /// </summary>
    public bool HasPublicKey => !string.IsNullOrWhiteSpace(PublicKey);

    /// <summary>
    /// True when a private key is present (decrypt).
    /// </summary>
    public bool HasPrivateKey => !string.IsNullOrWhiteSpace(PrivateKey);
}
