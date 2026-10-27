using OnlineStore.Application.Statistics;
using OnlineStore.Domain.Entities;
using OnlineStore.Domain.Enums;
using OnlineStore.Domain.ValueObjects;
using OnlineStore.Infrastructure.Repositories;

namespace OnlineStore.Application.Tests.Statistics;

public class OrderStatisticsServiceTests
{
    private readonly Customer _andrii = CreateCustomer("Andrii");
    private readonly Customer _bohdan = CreateCustomer("Bohdan");
    private readonly Customer _chrystyna = CreateCustomer("Chrystyna");

    private readonly Product _laptop = new("Laptop", 42000m);
    private readonly Product _mouse = new("Mouse", 800m);
    private readonly Product _cable = new("USB-C Cable", 350m);

    private readonly InMemoryRepository<Order> _orders = new();
    private readonly OrderStatisticsService _statistics;

    private readonly Order _andriiLaptop;
    private readonly Order _bohdanMice;
    private readonly Order _bohdanCancelledLaptop;
    private readonly Order _chrystynaUnpaid;
    private readonly Order _chrystynaCables;

    public OrderStatisticsServiceTests()
    {
        _statistics = new OrderStatisticsService(_orders);

        _andriiLaptop = AddOrder(_andrii, OrderStatus.Delivered, (_laptop, 1), (_mouse, 1)); // 42800
        AddOrder(_andrii, OrderStatus.Paid, (_cable, 2)); // 700
        AddOrder(_andrii, OrderStatus.Shipped, (_cable, 1)); // 350
        _bohdanMice = AddOrder(_bohdan, OrderStatus.Paid, (_mouse, 3)); // 2400
        _bohdanCancelledLaptop = AddOrder(_bohdan, OrderStatus.Cancelled, (_laptop, 1)); // 42000
        _chrystynaUnpaid = AddOrder(_chrystyna, OrderStatus.Created, (_mouse, 1), (_cable, 1)); // 1150
        _chrystynaCables = AddOrder(_chrystyna, OrderStatus.Paid, (_cable, 4)); // 1400
    }

    [Fact]
    public void GetOrdersWithTotalAbove_ReturnsOnlyMatchingOrders()
    {
        var result = _statistics.GetOrdersWithTotalAbove(1000m);

        Assert.Equal([_andriiLaptop, _bohdanMice, _bohdanCancelledLaptop, _chrystynaUnpaid, _chrystynaCables], result);
    }

    [Fact]
    public void GetMostExpensiveOrders_ReturnsOrdersByTotalDescending()
    {
        var result = _statistics.GetMostExpensiveOrders(3);

        Assert.Equal([_andriiLaptop, _bohdanCancelledLaptop, _bohdanMice], result);
    }

    [Fact]
    public void GetTotalSales_SumsOnlyPaidOrders()
    {
        Assert.Equal(47650m, _statistics.GetTotalSales());
    }

    [Fact]
    public void GetAverageOrderValue_AveragesOnlyPaidOrders()
    {
        Assert.Equal(9530m, _statistics.GetAverageOrderValue());
    }

    [Fact]
    public void GetOrderCountByCustomer_GroupsAllOrdersByCustomer()
    {
        var result = _statistics.GetOrderCountByCustomer();

        Assert.Equal(
            [
                new CustomerOrderCount(_andrii, 3),
                new CustomerOrderCount(_bohdan, 2),
                new CustomerOrderCount(_chrystyna, 2)
            ],
            result);
    }

    [Fact]
    public void GetOrderedProducts_ReturnsDistinctProductsFromAllOrders()
    {
        var result = _statistics.GetOrderedProducts();

        Assert.Equal([_laptop, _mouse, _cable], result);
    }

    [Fact]
    public void GetBestSellingProducts_ReturnsTopProductsBySoldQuantity()
    {
        var result = _statistics.GetBestSellingProducts(2);

        Assert.Equal([new ProductSales(_cable, 7), new ProductSales(_mouse, 4)], result);
    }

    [Fact]
    public void GetCustomersWithSpendingAbove_ReturnsCustomersWhoSpentMore()
    {
        var result = _statistics.GetCustomersWithSpendingAbove(5000m);

        Assert.Equal([new CustomerSpending(_andrii, 43850m)], result);
    }

    [Theory]
    [InlineData(10000, true)]
    [InlineData(42800, false)]
    public void HasOrderWithTotalAbove_ChecksAllOrders(decimal amount, bool expected)
    {
        Assert.Equal(expected, _statistics.HasOrderWithTotalAbove(amount));
    }

    [Fact]
    public void GetSpendingByCustomerId_ReturnsPaidTotalPerCustomer()
    {
        var result = _statistics.GetSpendingByCustomerId();

        Assert.Equal(3, result.Count);
        Assert.Equal(43850m, result[_andrii.Id]);
        Assert.Equal(2400m, result[_bohdan.Id]);
        Assert.Equal(1400m, result[_chrystyna.Id]);
    }

    [Fact]
    public void Statistics_WithoutOrders_ReturnEmptyResults()
    {
        var statistics = new OrderStatisticsService(new InMemoryRepository<Order>());

        Assert.Equal(0m, statistics.GetTotalSales());
        Assert.Equal(0m, statistics.GetAverageOrderValue());
        Assert.Empty(statistics.GetOrderCountByCustomer());
        Assert.Empty(statistics.GetBestSellingProducts(3));
        Assert.False(statistics.HasOrderWithTotalAbove(0m));
        Assert.Empty(statistics.GetSpendingByCustomerId());
    }

    private Order AddOrder(Customer customer, OrderStatus status, params (Product Product, int Quantity)[] items)
    {
        var order = new Order(customer);

        foreach (var (product, quantity) in items)
        {
            order.AddItem(product, quantity);
        }

        MoveToStatus(order, status);
        _orders.Add(order);

        return order;
    }

    private static void MoveToStatus(Order order, OrderStatus status)
    {
        if (status == OrderStatus.Cancelled)
        {
            order.Cancel();
            return;
        }

        if (status >= OrderStatus.Paid)
        {
            order.Pay(new PaymentReceipt("TEST-TRANSACTION", "Test payment", order.TotalAmount));
        }

        if (status >= OrderStatus.Shipped)
        {
            order.Ship();
        }

        if (status >= OrderStatus.Delivered)
        {
            order.Deliver();
        }
    }

    private static Customer CreateCustomer(string name) =>
        new()
        {
            Name = name,
            Email = $"{name.ToLowerInvariant()}@example.com",
            Address = new Address("Poltava", "Shevchenka", "12", "36000")
        };
}
