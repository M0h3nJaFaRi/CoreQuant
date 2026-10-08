using System.Security.Cryptography;
using System.Text;
using Framework;
using Framework.Security.Hashing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreQuant.Framework.UnitTests;

public sealed class HashServiceTests
{
    [Fact]
    public void ComputeSha256_Matches_Bcl_Reference()
    {
        var hash = CreateHashService();
        const string text = "service-foundation";

        var expected = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
        var actual = hash.ComputeSha256(text);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ComputeHmacSha256_Matches_Bcl_Reference()
    {
        var hash = CreateHashService();
        const string key = "secret";
        const string text = "payload";

        var expected = Convert.ToHexString(
                HMACSHA256.HashData(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(text)))
            .ToLowerInvariant();
        var actual = hash.ComputeHmacSha256(key, text);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AddFramework_Registers_IHashService_As_Singleton()
    {
        var services = new ServiceCollection();
        services.AddFramework(new ConfigurationBuilder().Build(), "test-service");
        using var provider = services.BuildServiceProvider();

        var first = provider.GetRequiredService<IHashService>();
        var second = provider.GetRequiredService<IHashService>();

        Assert.Same(first, second);
    }

    private static IHashService CreateHashService()
    {
        var services = new ServiceCollection();
        services.AddFramework(new ConfigurationBuilder().Build(), "test-service");
        return services.BuildServiceProvider().GetRequiredService<IHashService>();
    }
}
