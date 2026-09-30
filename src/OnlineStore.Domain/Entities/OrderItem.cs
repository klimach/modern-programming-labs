using OnlineStore.Domain.Exceptions;

namespace OnlineStore.Domain.Entities;

public sealed class OrderItem
{
    public Product Product { get; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; }
    public decimal Total => UnitPrice * Quantity;

    internal OrderItem(Product product, int quantity)
    {
        ArgumentNullException.ThrowIfNull(product);
        EnsureQuantityIsPositive(quantity);

        Product = product;
        Quantity = quantity;
        UnitPrice = product.Price;
    }

    internal void IncreaseQuantity(int quantity)
    {
        EnsureQuantityIsPositive(quantity);

        Quantity += quantity;
    }

    private static void EnsureQuantityIsPositive(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Quantity must be greater than zero.");
        }
    }
}
