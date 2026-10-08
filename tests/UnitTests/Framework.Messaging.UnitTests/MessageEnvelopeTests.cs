using Framework.Messaging.Envelope;

namespace Framework.Messaging.UnitTests;

public sealed class MessageEnvelopeTests
{
    [Fact]
    public void Create_Sets_MessageType_Payload_And_Timestamp()
    {
        var occurred = new DateTimeOffset(2026, 9, 20, 12, 0, 0, TimeSpan.Zero);
        var envelope = MessageEnvelope<string>.Create("demo.message", "body", occurred, "corr-1");

        Assert.Equal("demo.message", envelope.MessageType);
        Assert.Equal("body", envelope.Payload);
        Assert.Equal(occurred, envelope.OccurredOnUtc);
        Assert.Equal("corr-1", envelope.CorrelationId);
        Assert.NotEqual(Guid.Empty, envelope.MessageId);
    }

    [Fact]
    public void Constructor_Rejects_Empty_MessageType()
    {
        Assert.Throws<ArgumentException>(() =>
            new MessageEnvelope<string>(Guid.NewGuid(), " ", DateTimeOffset.UtcNow, "x"));
    }
}
