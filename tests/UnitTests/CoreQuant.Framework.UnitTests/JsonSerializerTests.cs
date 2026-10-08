using Framework;
using Framework.Serialization.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreQuant.Framework.UnitTests;

public sealed class JsonSerializerTests
{
    private sealed record SampleDto(string Name, int Count);

    [Fact]
    public void RoundTrip_Serialize_Deserialize_Preserves_Values()
    {
        var serializer = CreateSerializer();
        var original = new SampleDto("alpha", 3);

        var json = serializer.Serialize(original);
        var restored = serializer.Deserialize<SampleDto>(json);

        Assert.Equal(original, restored);
        Assert.Contains("name", json, StringComparison.Ordinal);
    }

    [Fact]
    public void RoundTrip_Utf8Bytes_Preserves_Values()
    {
        var serializer = CreateSerializer();
        var original = new SampleDto("beta", 7);

        var bytes = serializer.SerializeToUtf8Bytes(original);
        var restored = serializer.Deserialize<SampleDto>(bytes);

        Assert.Equal(original, restored);
    }

    [Fact]
    public void AddFramework_Registers_IJsonSerializer_As_Singleton()
    {
        var services = new ServiceCollection();
        services.AddFramework(new ConfigurationBuilder().Build(), "test-service");
        using var provider = services.BuildServiceProvider();

        var first = provider.GetRequiredService<IJsonSerializer>();
        var second = provider.GetRequiredService<IJsonSerializer>();

        Assert.Same(first, second);
    }

    private static IJsonSerializer CreateSerializer()
    {
        var services = new ServiceCollection();
        services.AddFramework(new ConfigurationBuilder().Build(), "test-service");
        return services.BuildServiceProvider().GetRequiredService<IJsonSerializer>();
    }
}
