using OnlineStore.Domain.Exceptions;

namespace OnlineStore.Domain.ValueObjects;

public sealed class PaymentReceipt
{
    public string TransactionId { get; }
    public string PaymentMethod { get; }
    public decimal Amount { get; }

    public PaymentReceipt(string transactionId, string paymentMethod, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
        {
            throw new DomainException("Transaction id cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(paymentMethod))
        {
            throw new DomainException("Payment method cannot be empty.");
        }

        if (amount <= 0)
        {
            throw new DomainException("Payment amount must be greater than zero.");
        }

        TransactionId = transactionId;
        PaymentMethod = paymentMethod;
        Amount = amount;
    }
}
