using OnlineStore.Application.Payments;
using OnlineStore.Domain.Entities;
using OnlineStore.Domain.ValueObjects;

namespace OnlineStore.Application.Orders;

public sealed class OrderPaymentService
{
    public PaymentReceipt Pay(Order order, IPaymentMethod paymentMethod)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(paymentMethod);

        order.EnsureCanBePaid();

        var receipt = paymentMethod.Pay(order.TotalAmount);
        order.Pay(receipt);

        return receipt;
    }
}
