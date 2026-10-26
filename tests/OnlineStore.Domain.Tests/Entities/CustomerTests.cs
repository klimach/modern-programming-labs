using OnlineStore.Domain.Entities;
using OnlineStore.Domain.Exceptions;

namespace OnlineStore.Domain.Tests.Entities;

public class CustomerTests
{
    [Fact]
    public void Initializer_WithValidData_CreatesCustomer()
    {
        var customer = new Customer { Name = "Oleksandr Klimach", Email = "oleksandr@example.com" };

        Assert.Equal("Oleksandr Klimach", customer.Name);
        Assert.Equal("oleksandr@example.com", customer.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Initializer_WithEmptyName_ThrowsDomainException(string name)
    {
        Assert.Throws<DomainException>(() => new Customer { Name = name, Email = "oleksandr@example.com" });
    }

    [Theory]
    [InlineData("")]
    [InlineData("oleksandr")]
    [InlineData("oleksandr@")]
    public void Initializer_WithInvalidEmail_ThrowsDomainException(string email)
    {
        Assert.Throws<DomainException>(() => new Customer { Name = "Oleksandr Klimach", Email = email });
    }
}
