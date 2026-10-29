using OnlineStore.Application.Repositories;
using OnlineStore.Domain.Entities;

namespace OnlineStore.Application.Statistics;

// Sales figures (totals, averages, spending, best sellers) count only paid orders:
// created and cancelled orders have no payment and did not bring any revenue.
public sealed class OrderStatisticsService(IRepository<Order> orders)
{
    private IEnumerable<Order> Orders => orders.GetAll();

    private IEnumerable<Order> PaidOrders => Orders.Where(order => order.Payment is not null);

    public IReadOnlyList<Order> GetOrdersWithTotalAbove(decimal amount) =>
        Orders
            .Where(order => order.TotalAmount > amount)
            .ToList();

    public IReadOnlyList<Order> GetMostExpensiveOrders(int count) =>
        Orders
            .OrderByDescending(order => order.TotalAmount)
            .Take(count)
            .ToList();

    public decimal GetTotalSales() => PaidOrders.Sum(order => order.TotalAmount);

    public decimal GetAverageOrderValue() =>
        PaidOrders
            .Select(order => order.TotalAmount)
            .DefaultIfEmpty()
            .Average();

    public IReadOnlyList<CustomerOrderCount> GetOrderCountByCustomer() =>
        Orders
            .GroupBy(order => order.Customer)
            .Select(group => new CustomerOrderCount(group.Key, group.Count()))
            .OrderByDescending(result => result.OrderCount)
            .ThenBy(result => result.Customer.Name)
            .ToList();

    public IReadOnlyList<Product> GetOrderedProducts() =>
        Orders
            .SelectMany(order => order.Items)
            .Select(item => item.Product)
            .Distinct()
            .OrderBy(product => product.Name)
            .ToList();

    public IReadOnlyList<ProductSales> GetBestSellingProducts(int count) =>
        PaidOrders
            .SelectMany(order => order.Items)
            .GroupBy(item => item.Product)
            .Select(group => new ProductSales(group.Key, group.Sum(item => item.Quantity)))
            .OrderByDescending(result => result.QuantitySold)
            .ThenBy(result => result.Product.Name)
            .Take(count)
            .ToList();

    public IReadOnlyList<CustomerSpending> GetCustomersWithSpendingAbove(decimal amount) =>
        PaidOrders
            .GroupBy(order => order.Customer)
            .Select(group => new CustomerSpending(group.Key, group.Sum(order => order.TotalAmount)))
            .Where(result => result.TotalSpent > amount)
            .OrderByDescending(result => result.TotalSpent)
            .ToList();

    public bool HasOrderWithTotalAbove(decimal amount) => Orders.Any(order => order.TotalAmount > amount);

    public IReadOnlyDictionary<Guid, decimal> GetSpendingByCustomerId() =>
        PaidOrders
            .GroupBy(order => order.Customer.Id)
            .ToDictionary(group => group.Key, group => group.Sum(order => order.TotalAmount));
}
