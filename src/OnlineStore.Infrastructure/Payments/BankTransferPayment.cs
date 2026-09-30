using OnlineStore.Application.Payments;
using OnlineStore.Domain.ValueObjects;

namespace OnlineStore.Infrastructure.Payments;

public sealed class BankTransferPayment : IPaymentMethod
{
    private const string UkrainianCountryCode = "UA";
    private const int UkrainianIbanLength = 29;

    private readonly string _iban;

    public string Name => $"Bank transfer {_iban[..4]}...{_iban[^4..]}";

    public BankTransferPayment(string iban)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(iban);

        if (iban.Length != UkrainianIbanLength
            || !iban.StartsWith(UkrainianCountryCode, StringComparison.Ordinal)
            || !iban[UkrainianCountryCode.Length..].All(char.IsAsciiDigit)
            || !PassesIbanCheck(iban))
        {
            throw new ArgumentException("IBAN is invalid.", nameof(iban));
        }

        _iban = iban;
    }

    public PaymentReceipt Pay(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Payment amount must be greater than zero.");
        }

        // A real implementation would create a transfer through a banking API.
        return new PaymentReceipt($"BANK-{Guid.NewGuid():N}", Name, amount);
    }

    // ISO 13616: move the first four characters to the end, convert letters to numbers (A = 10 ... Z = 35),
    // and the remainder of dividing the result by 97 must be 1.
    private static bool PassesIbanCheck(string iban)
    {
        var rearranged = iban[4..] + iban[..4];
        var remainder = 0;

        foreach (var character in rearranged)
        {
            remainder = char.IsAsciiDigit(character)
                ? (remainder * 10 + (character - '0')) % 97
                : (remainder * 100 + (character - 'A' + 10)) % 97;
        }

        return remainder == 1;
    }
}
