using OnlineStore.Domain.ValueObjects;

namespace OnlineStore.Application.Payments;

public interface IPaymentMethod
{
    string Name { get; }

    PaymentReceipt Pay(decimal amount);
}
