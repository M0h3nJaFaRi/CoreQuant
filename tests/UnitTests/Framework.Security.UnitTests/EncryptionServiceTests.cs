using System.Security.Cryptography;
using Framework.Security;
using Framework.Security.Configuration.Options;
using Framework.Security.Encryption;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Framework.Security.UnitTests;

public sealed class EncryptionServiceTests
{
    [Fact]
    public void Aes_RoundTrips_PlainText()
    {
        using var aes = Aes.Create();
        aes.GenerateKey();
        aes.GenerateIV();

        var services = new ServiceCollection();
        services.AddFrameworkSecurity();
        services.Configure<EncryptionOptions>(options =>
        {
            options.AES.Key = Convert.ToBase64String(aes.Key);
            options.AES.IV = Convert.ToBase64String(aes.IV);
        });

        using var provider = services.BuildServiceProvider();
        var encryption = provider.GetRequiredService<IAesEncryptionService>();

        const string plain = "service-foundation-secret";
        var cipher = encryption.EncryptToBase64(plain);
        var restored = encryption.DecryptFromBase64(cipher);

        Assert.False(string.Equals(plain, cipher, StringComparison.Ordinal));
        Assert.Equal(plain, restored);
    }

    [Fact]
    public void Rsa_RoundTrips_PlainText()
    {
        using var rsa = RSA.Create(2048);
        var publicPem = rsa.ExportSubjectPublicKeyInfoPem();
        var privatePem = rsa.ExportPkcs8PrivateKeyPem();

        var services = new ServiceCollection();
        services.AddFrameworkSecurity();
        services.Configure<EncryptionOptions>(options =>
        {
            options.RSA.PublicKey = publicPem;
            options.RSA.PrivateKey = privatePem;
        });

        using var provider = services.BuildServiceProvider();
        var encryption = provider.GetRequiredService<IRsaEncryptionService>();

        const string plain = "rsa-payload";
        var cipher = encryption.EncryptToBase64(plain);
        var restored = encryption.DecryptFromBase64(cipher);

        Assert.False(string.Equals(plain, cipher, StringComparison.Ordinal));
        Assert.Equal(plain, restored);
    }

    [Fact]
    public void Aes_Without_Keys_Throws()
    {
        var services = new ServiceCollection();
        services.AddFrameworkSecurity();
        using var provider = services.BuildServiceProvider();
        var encryption = provider.GetRequiredService<IAesEncryptionService>();

        Assert.Throws<InvalidOperationException>(() => encryption.EncryptToBase64("x"));
    }

    [Fact]
    public void AddFrameworkSecurity_Registers_Encryption_Singletons()
    {
        var services = new ServiceCollection();
        services.AddFrameworkSecurity();
        using var provider = services.BuildServiceProvider();

        Assert.Same(
            provider.GetRequiredService<IAesEncryptionService>(),
            provider.GetRequiredService<IAesEncryptionService>());
        Assert.Same(
            provider.GetRequiredService<IRsaEncryptionService>(),
            provider.GetRequiredService<IRsaEncryptionService>());
        Assert.NotNull(provider.GetRequiredService<IOptions<EncryptionOptions>>());
    }
}
