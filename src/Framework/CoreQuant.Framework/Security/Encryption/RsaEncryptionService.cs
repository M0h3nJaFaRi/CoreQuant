using System.Text;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using Framework.Security.Configuration.Options;

namespace Framework.Security.Encryption;

/// <summary>
/// Default <see cref="IRsaEncryptionService"/> using BCL <see cref="RSA"/>.
/// </summary>
internal sealed class RsaEncryptionService(IOptionsMonitor<EncryptionOptions> options) : IRsaEncryptionService
{
    /// <inheritdoc />
    public byte[] Encrypt(ReadOnlySpan<byte> plainText)
    {
        using var rsa = CreatePublicRsa();
        return rsa.Encrypt(plainText.ToArray(), RSAEncryptionPadding.OaepSHA256);
    }

    /// <inheritdoc />
    public byte[] Decrypt(ReadOnlySpan<byte> cipherText)
    {
        using var rsa = CreatePrivateRsa();
        return rsa.Decrypt(cipherText.ToArray(), RSAEncryptionPadding.OaepSHA256);
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

    private RSA CreatePublicRsa()
    {
        var rsaOptions = options.CurrentValue.RSA;
        if (!rsaOptions.HasPublicKey)
            throw new InvalidOperationException(
                $"{EncryptionOptions.SectionName}:RSA PublicKey must be configured before encrypt.");

        var rsa = RSA.Create();
        rsa.ImportFromPem(rsaOptions.PublicKey);
        return rsa;
    }

    private RSA CreatePrivateRsa()
    {
        var rsaOptions = options.CurrentValue.RSA;
        if (!rsaOptions.HasPrivateKey)
            throw new InvalidOperationException(
                $"{EncryptionOptions.SectionName}:RSA PrivateKey must be configured before decrypt.");

        var rsa = RSA.Create();
        rsa.ImportFromPem(rsaOptions.PrivateKey);
        return rsa;
    }
}
