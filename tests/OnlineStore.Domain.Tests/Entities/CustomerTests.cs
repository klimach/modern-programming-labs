using OnlineStore.Domain.Entities;
using OnlineStore.Domain.Exceptions;
using OnlineStore.Domain.ValueObjects;

namespace OnlineStore.Domain.Tests.Entities;

public class CustomerTests
{
    [Fact]
    public void Initializer_WithValidData_CreatesCustomer()
    {
        var customer = new Customer { Name = "Oleksandr Klimach", Email = "oleksandr@example.com" };

        Assert.Equal("Oleksandr Klimach", customer.Name);
        Assert.Equal("oleksandr@example.com", customer.Email);
        Assert.Null(customer.Address);
        Assert.Null(customer.PhoneNumber);
    }

    [Fact]
    public void Initializer_WithOptionalData_SetsAddressAndPhoneNumber()
    {
        var address = new Address("Poltava", "Shevchenka", "12", "36000");

        var customer = new Customer
        {
            Name = "Oleksandr Klimach",
            Email = "oleksandr@example.com",
            Address = address,
            PhoneNumber = "+380501234567"
        };

        Assert.Equal(address, customer.Address);
        Assert.Equal("+380501234567", customer.PhoneNumber);
    }

    [Theory]
    [InlineData("")]
    [InlineData("380501234567")]
    [InlineData("+38050")]
    [InlineData("+38050123456A")]
    public void Initializer_WithInvalidPhoneNumber_ThrowsDomainException(string phoneNumber)
    {
        Assert.Throws<DomainException>(() => new Customer
        {
            Name = "Oleksandr Klimach",
            Email = "oleksandr@example.com",
            PhoneNumber = phoneNumber
        });
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
