using OnlineStore.Domain.Common;
using OnlineStore.Domain.Exceptions;

namespace OnlineStore.Domain.Entities;

public sealed class Product : Entity
{
    public string Name { get; }
    public decimal Price { get; }

    public Product(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Product name cannot be empty.");
        }

        if (price <= 0)
        {
            throw new DomainException("Product price must be greater than zero.");
        }

        Name = name;
        Price = price;
    }
}
