using Common.Domain.Exceptions;
using Common.Domain.Guards;

namespace CoreQuant.Commen.UnitTests;

public sealed class EnsureTests
{
    [Fact]
    public void That_Throws_DomainException_When_False()
    {
        var ex = Assert.Throws<DomainException>(() => Ensure.That(false, "bad", "bad_code"));
        Assert.Equal("bad_code", ex.Code);
    }

    [Fact]
    public void That_Does_Not_Throw_When_True()
    {
        Ensure.That(true, "ok", "ok_code");
    }
}
