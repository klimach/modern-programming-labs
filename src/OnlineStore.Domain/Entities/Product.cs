using OnlineStore.Domain.Common;
using OnlineStore.Domain.Exceptions;

namespace OnlineStore.Domain.Entities;

public sealed class Product : Entity
{
    public string Name { get; }
    public decimal Price { get; private set; }

    public Product(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Product name cannot be empty.");
        }

        EnsurePriceIsPositive(price);

        Name = name;
        Price = price;
    }

    public void ChangePrice(decimal newPrice)
    {
        EnsurePriceIsPositive(newPrice);

        Price = newPrice;
    }

    private static void EnsurePriceIsPositive(decimal price)
    {
        if (price <= 0)
        {
            throw new DomainException("Product price must be greater than zero.");
        }
    }
}
