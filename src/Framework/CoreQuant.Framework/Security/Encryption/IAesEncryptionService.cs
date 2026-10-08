namespace Framework.Security.Encryption;

/// <summary>
/// Symmetric AES encryption / decryption using configured key material.
/// </summary>
public interface IAesEncryptionService
{
    /// <summary>
    /// Encrypts <paramref name="plainText"/> with AES-CBC and returns ciphertext bytes.
    /// </summary>
    byte[] Encrypt(ReadOnlySpan<byte> plainText);

    /// <summary>
    /// Decrypts AES-CBC <paramref name="cipherText"/> and returns plaintext bytes.
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
