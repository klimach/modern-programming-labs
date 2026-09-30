using OnlineStore.Domain.Entities;
using OnlineStore.Domain.Exceptions;

namespace OnlineStore.Domain.Tests.Entities;

public class ProductTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesProduct()
    {
        var product = new Product("Laptop", 45000m);

        Assert.Equal("Laptop", product.Name);
        Assert.Equal(45000m, product.Price);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithNonPositivePrice_ThrowsDomainException(decimal price)
    {
        Assert.Throws<DomainException>(() => new Product("Laptop", price));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyName_ThrowsDomainException(string name)
    {
        Assert.Throws<DomainException>(() => new Product(name, 100m));
    }

    [Fact]
    public void ChangePrice_WithPositivePrice_UpdatesPrice()
    {
        var product = new Product("Laptop", 45000m);

        product.ChangePrice(42000m);

        Assert.Equal(42000m, product.Price);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ChangePrice_WithNonPositivePrice_ThrowsDomainException(decimal price)
    {
        var product = new Product("Laptop", 45000m);

        Assert.Throws<DomainException>(() => product.ChangePrice(price));
        Assert.Equal(45000m, product.Price);
    }
}
