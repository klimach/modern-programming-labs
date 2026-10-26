using OnlineStore.Domain.Exceptions;
using OnlineStore.Domain.ValueObjects;

namespace OnlineStore.Domain.Tests.ValueObjects;

public class AddressTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesAddress()
    {
        var address = new Address("Poltava", "Shevchenka", "12", "36000");

        Assert.Equal("Poltava", address.City);
        Assert.Equal("Shevchenka", address.Street);
        Assert.Equal("12", address.Building);
        Assert.Equal("36000", address.PostalCode);
    }

    [Fact]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        var first = new Address("Poltava", "Shevchenka", "12", "36000");
        var second = new Address("Poltava", "Shevchenka", "12", "36000");

        Assert.Equal(first, second);
        Assert.True(first == second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void Equals_WithDifferentValues_ReturnsFalse()
    {
        var first = new Address("Poltava", "Shevchenka", "12", "36000");
        var second = new Address("Poltava", "Shevchenka", "14", "36000");

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void With_CreatesChangedCopy()
    {
        var address = new Address("Poltava", "Shevchenka", "12", "36000");

        var changed = address with { Building = "14" };

        Assert.Equal("12", address.Building);
        Assert.Equal("14", changed.Building);
        Assert.Equal(address.City, changed.City);
    }

    [Theory]
    [InlineData("", "Shevchenka", "12")]
    [InlineData("Poltava", "  ", "12")]
    [InlineData("Poltava", "Shevchenka", "")]
    public void Constructor_WithEmptyPart_ThrowsDomainException(string city, string street, string building)
    {
        Assert.Throws<DomainException>(() => new Address(city, street, building, "36000"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("3600")]
    [InlineData("360000")]
    [InlineData("36A00")]
    public void Constructor_WithInvalidPostalCode_ThrowsDomainException(string postalCode)
    {
        Assert.Throws<DomainException>(() => new Address("Poltava", "Shevchenka", "12", postalCode));
    }
}
