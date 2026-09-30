using OnlineStore.Application.Payments;
using OnlineStore.Domain.ValueObjects;

namespace OnlineStore.Infrastructure.Payments;

public sealed class CardPayment : IPaymentMethod
{
    private const int CardNumberLength = 16;

    private readonly string _lastFourDigits;

    public string Name => $"Card **** {_lastFourDigits}";

    public CardPayment(string cardNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cardNumber);

        if (cardNumber.Length != CardNumberLength
            || !cardNumber.All(char.IsAsciiDigit)
            || !PassesLuhnCheck(cardNumber))
        {
            throw new ArgumentException("Card number is invalid.", nameof(cardNumber));
        }

        _lastFourDigits = cardNumber[^4..];
    }

    public PaymentReceipt Pay(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Payment amount must be greater than zero.");
        }

        // A real implementation would charge the card through a payment gateway.
        return new PaymentReceipt($"CARD-{Guid.NewGuid():N}", Name, amount);
    }

    private static bool PassesLuhnCheck(string cardNumber)
    {
        var sum = 0;

        for (var i = 0; i < cardNumber.Length; i++)
        {
            var digit = cardNumber[cardNumber.Length - 1 - i] - '0';

            if (i % 2 == 1)
            {
                digit *= 2;

                if (digit > 9)
                {
                    digit -= 9;
                }
            }

            sum += digit;
        }

        return sum % 10 == 0;
    }
}
