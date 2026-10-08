namespace Framework.Security.Encryption;

/// <summary>
/// Asymmetric RSA encryption / decryption using configured PEM keys.
/// </summary>
public interface IRsaEncryptionService
{
    /// <summary>
    /// Encrypts <paramref name="plainText"/> with the configured RSA public key (OAEP SHA-256).
    /// </summary>
    byte[] Encrypt(ReadOnlySpan<byte> plainText);

    /// <summary>
    /// Decrypts <paramref name="cipherText"/> with the configured RSA private key (OAEP SHA-256).
    /// </summary>
    byte[] Decrypt(ReadOnlySpan<byte> cipherText);

    /// <summary>
    /// Encrypts UTF-8 text and returns a Base64 ciphertext string.
    /// </summary>
    string EncryptToBase64(string plainText);

    /// <summary>
    /// Decrypts a Base64 ciphertext string and returns UTF-8 plaintext.
    /// </summary>
    string DecryptFromBase64(string cipherTextBase64);
}
