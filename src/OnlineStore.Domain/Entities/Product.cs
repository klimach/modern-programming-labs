namespace OnlineStore.Domain.Entities;

public sealed class Product
{
    public Guid Id { get; } = Guid.CreateVersion7();
    public string Name { get; }
    public decimal Price { get; }

    public Product(string name, decimal price)
    {
        Name = name;
        Price = price;
    }
}
