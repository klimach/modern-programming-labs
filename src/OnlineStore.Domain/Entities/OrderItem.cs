namespace OnlineStore.Domain.Entities;

public sealed class OrderItem
{
    public Product Product { get; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; }
    public decimal Total => UnitPrice * Quantity;

    public OrderItem(Product product, int quantity)
    {
        Product = product;
        Quantity = quantity;
        UnitPrice = product.Price;
    }
}
