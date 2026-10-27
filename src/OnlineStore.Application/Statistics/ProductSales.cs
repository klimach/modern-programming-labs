using OnlineStore.Domain.Entities;

namespace OnlineStore.Application.Statistics;

public sealed record ProductSales(Product Product, int QuantitySold);
