using OnlineStore.Domain.Common;
using OnlineStore.Domain.Enums;
using OnlineStore.Domain.Exceptions;
using OnlineStore.Domain.ValueObjects;

namespace OnlineStore.Domain.Entities;

public sealed class Order : Entity
{
    private const decimal FreeDeliveryThreshold = 2000m;

    private readonly List<OrderItem> _items = [];

    public Customer Customer { get; }
    public OrderStatus Status { get; private set; } = OrderStatus.Created;
    public PaymentReceipt? Payment { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public decimal TotalAmount => _items.Sum(item => item.Total);
    public bool HasFreeDelivery => this is { Status: OrderStatus.Paid, TotalAmount: >= FreeDeliveryThreshold };

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
        var error = Status switch
        {
            OrderStatus.Paid or OrderStatus.Shipped or OrderStatus.Delivered => "Order is already paid.",
            OrderStatus.Cancelled => "Cancelled order cannot be paid.",
            _ when _items.Count == 0 => "Empty order cannot be paid.",
            _ => null
        };

        if (error is not null)
        {
            throw new DomainException(error);
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

    public void Ship()
    {
        if (Status != OrderStatus.Paid)
        {
            throw new DomainException($"Order with status {Status} cannot be shipped.");
        }

        if (Customer.Address is null)
        {
            throw new DomainException("Order cannot be shipped without a delivery address.");
        }

        Status = OrderStatus.Shipped;
    }

    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
        {
            throw new DomainException($"Order with status {Status} cannot be delivered.");
        }

        Status = OrderStatus.Delivered;
    }

    public void Cancel()
    {
        Status = Status switch
        {
            OrderStatus.Created => OrderStatus.Cancelled,
            OrderStatus.Cancelled => throw new DomainException("Order is already cancelled."),
            _ => throw new DomainException($"{Status} order cannot be cancelled.")
        };
    }

    public string GetStatusDescription() => Status switch
    {
        OrderStatus.Created => "Order is created and awaiting payment",
        OrderStatus.Paid => "Order is paid and being prepared for shipment",
        OrderStatus.Shipped => "Order is shipped and on its way to the customer",
        OrderStatus.Delivered => "Order is delivered to the customer",
        OrderStatus.Cancelled => "Order is cancelled",
        _ => "Unknown status"
    };

    public OrderCategory GetOrderCategory() => TotalAmount switch
    {
        < 500m => OrderCategory.Small,
        < 5000m => OrderCategory.Medium,
        _ => OrderCategory.Large
    };

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
