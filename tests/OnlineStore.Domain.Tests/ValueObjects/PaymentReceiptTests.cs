using OnlineStore.Domain.Exceptions;
using OnlineStore.Domain.ValueObjects;

namespace OnlineStore.Domain.Tests.ValueObjects;

public class PaymentReceiptTests
{
    [Fact]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        var first = new PaymentReceipt("CARD-1", "Card **** 1111", 800m);
        var second = new PaymentReceipt("CARD-1", "Card **** 1111", 800m);

        Assert.Equal(first, second);
    }

    [Theory]
    [InlineData("", "Card **** 1111", 800)]
    [InlineData("CARD-1", " ", 800)]
    [InlineData("CARD-1", "Card **** 1111", 0)]
    [InlineData("CARD-1", "Card **** 1111", -1)]
    public void Constructor_WithInvalidData_ThrowsDomainException(string transactionId, string paymentMethod, decimal amount)
    {
        Assert.Throws<DomainException>(() => new PaymentReceipt(transactionId, paymentMethod, amount));
    }
}
