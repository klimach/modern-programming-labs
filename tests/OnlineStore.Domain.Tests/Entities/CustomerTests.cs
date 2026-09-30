using OnlineStore.Domain.Entities;
using OnlineStore.Domain.Exceptions;

namespace OnlineStore.Domain.Tests.Entities;

public class CustomerTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesCustomer()
    {
        var customer = new Customer("Oleksandr Klimach", "oleksandr@example.com");

        Assert.Equal("Oleksandr Klimach", customer.FullName);
        Assert.Equal("oleksandr@example.com", customer.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyName_ThrowsDomainException(string fullName)
    {
        Assert.Throws<DomainException>(() => new Customer(fullName, "oleksandr@example.com"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("oleksandr")]
    [InlineData("oleksandr@")]
    public void Constructor_WithInvalidEmail_ThrowsDomainException(string email)
    {
        Assert.Throws<DomainException>(() => new Customer("Oleksandr Klimach", email));
    }
}
