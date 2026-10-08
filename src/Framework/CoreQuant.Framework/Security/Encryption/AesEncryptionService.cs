using System.Text;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using Framework.Security.Configuration.Options;

namespace Framework.Security.Encryption;

/// <summary>
/// Default <see cref="IAesEncryptionService"/> using BCL <see cref="Aes"/>.
/// </summary>
internal sealed class AesEncryptionService(IOptionsMonitor<EncryptionOptions> options) : IAesEncryptionService
{
    /// <inheritdoc />
    public byte[] Encrypt(ReadOnlySpan<byte> plainText)
    {
        var (key, iv) = ResolveKeyMaterial();
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        return aes.EncryptCbc(plainText, iv, PaddingMode.PKCS7);
    }

    /// <inheritdoc />
    public byte[] Decrypt(ReadOnlySpan<byte> cipherText)
    {
        var (key, iv) = ResolveKeyMaterial();
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        return aes.DecryptCbc(cipherText, iv, PaddingMode.PKCS7);
    }

    /// <inheritdoc />
    public string EncryptToBase64(string plainText)
    {
        ArgumentNullException.ThrowIfNull(plainText);
        return Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(plainText)));
    }

    /// <inheritdoc />
    public string DecryptFromBase64(string cipherTextBase64)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cipherTextBase64);
        var plain = Decrypt(Convert.FromBase64String(cipherTextBase64));
        return Encoding.UTF8.GetString(plain);
    }

    private (byte[] Key, byte[] Iv) ResolveKeyMaterial()
    {
        var aes = options.CurrentValue.AES;
        if (!aes.IsConfigured)
            throw new InvalidOperationException(
                $"{EncryptionOptions.SectionName}:AES Key and IV must be configured before use.");

        return (Convert.FromBase64String(aes.Key), Convert.FromBase64String(aes.IV));
    }
}
