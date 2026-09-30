using OnlineStore.Domain.Common;
using OnlineStore.Domain.Exceptions;

namespace OnlineStore.Domain.Tests.Common;

public class EntityTests
{
    [Fact]
    public void Equals_WithSameId_ReturnsTrue()
    {
        var id = Guid.NewGuid();

        var first = new TestEntity(id);
        var second = new TestEntity(id);

        Assert.Equal(first, second);
        Assert.True(first == second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void Equals_WithDifferentIds_ReturnsFalse()
    {
        var first = new TestEntity(Guid.NewGuid());
        var second = new TestEntity(Guid.NewGuid());

        Assert.NotEqual(first, second);
        Assert.True(first != second);
    }

    [Fact]
    public void Equals_WithDifferentTypesAndSameId_ReturnsFalse()
    {
        var id = Guid.NewGuid();

        Entity first = new TestEntity(id);
        Entity second = new OtherTestEntity(id);

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Constructor_WithEmptyId_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => new TestEntity(Guid.Empty));
    }

    private sealed class TestEntity(Guid id) : Entity(id);

    private sealed class OtherTestEntity(Guid id) : Entity(id);
}
