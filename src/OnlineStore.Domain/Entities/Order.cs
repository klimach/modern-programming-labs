using OnlineStore.Domain.Enums;

namespace OnlineStore.Domain.Entities;

public sealed class Order
{
    private readonly List<OrderItem> _items = [];

    public Guid Id { get; } = Guid.CreateVersion7();
    public Customer Customer { get; }
    public OrderStatus Status { get; private set; } = OrderStatus.Created;
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public decimal TotalAmount => _items.Sum(item => item.Total);

    public Order(Customer customer)
    {
        Customer = customer;
    }
}
