using OnlineStore.Domain.Exceptions;

namespace OnlineStore.Domain.ValueObjects;

public sealed record PaymentReceipt(string TransactionId, string PaymentMethod, decimal Amount)
{
    public string TransactionId { get; init; } = !string.IsNullOrWhiteSpace(TransactionId)
        ? TransactionId
        : throw new DomainException("Transaction id cannot be empty.");

    public string PaymentMethod { get; init; } = !string.IsNullOrWhiteSpace(PaymentMethod)
        ? PaymentMethod
        : throw new DomainException("Payment method cannot be empty.");

    public decimal Amount { get; init; } = Amount > 0
        ? Amount
        : throw new DomainException("Payment amount must be greater than zero.");
}
