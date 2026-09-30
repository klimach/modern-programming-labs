using OnlineStore.Domain.Common;
using OnlineStore.Domain.Enums;
using OnlineStore.Domain.Exceptions;
using OnlineStore.Domain.ValueObjects;

namespace OnlineStore.Domain.Entities;

public sealed class Order : Entity
{
    private readonly List<OrderItem> _items = [];

    public Customer Customer { get; }
    public OrderStatus Status { get; private set; } = OrderStatus.Created;
    public PaymentReceipt? Payment { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public decimal TotalAmount => _items.Sum(item => item.Total);

    public Order(Customer customer)
    {
        ArgumentNullException.ThrowIfNull(customer);

        Customer = customer;
    }

    public void AddItem(Product product, int quantity)
    {
        ArgumentNullException.ThrowIfNull(product);
        EnsureCanBeModified();

        var existingItem = _items.FirstOrDefault(item => item.Product == product);

        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(quantity);
            return;
        }

        _items.Add(new OrderItem(product, quantity));
    }

    public void RemoveItem(Guid productId)
    {
        EnsureCanBeModified();

        _items.Remove(GetItem(productId));
    }

    public void ChangeItemQuantity(Guid productId, int quantity)
    {
        EnsureCanBeModified();

        GetItem(productId).ChangeQuantity(quantity);
    }

    public void EnsureCanBePaid()
    {
        if (Status == OrderStatus.Paid)
        {
            throw new DomainException("Order is already paid.");
        }

        if (Status == OrderStatus.Cancelled)
        {
            throw new DomainException("Cancelled order cannot be paid.");
        }

        if (_items.Count == 0)
        {
            throw new DomainException("Empty order cannot be paid.");
        }
    }

    public void Pay(PaymentReceipt payment)
    {
        ArgumentNullException.ThrowIfNull(payment);
        EnsureCanBePaid();

        if (payment.Amount != TotalAmount)
        {
            throw new DomainException("Payment amount does not match the order total.");
        }

        Payment = payment;
        Status = OrderStatus.Paid;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Paid)
        {
            throw new DomainException("Paid order cannot be cancelled.");
        }

        if (Status == OrderStatus.Cancelled)
        {
            throw new DomainException("Order is already cancelled.");
        }

        Status = OrderStatus.Cancelled;
    }

    private OrderItem GetItem(Guid productId) =>
        _items.FirstOrDefault(item => item.Product.Id == productId)
            ?? throw new DomainException("Product is not in the order.");

    private void EnsureCanBeModified()
    {
        if (Status != OrderStatus.Created)
        {
            throw new DomainException($"Order with status {Status} cannot be modified.");
        }
    }
}
