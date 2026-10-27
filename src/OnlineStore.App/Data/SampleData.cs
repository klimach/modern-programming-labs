using OnlineStore.Application.Orders;
using OnlineStore.Application.Repositories;
using OnlineStore.Domain.Entities;
using OnlineStore.Domain.Enums;
using OnlineStore.Domain.ValueObjects;
using OnlineStore.Infrastructure.Payments;

namespace OnlineStore.App.Data;

public static class SampleData
{
    private const string CardNumber = "4111111111111111";

    private static readonly OrderPaymentService PaymentService = new();

    public static void Seed(IRepository<Customer> customers, IRepository<Product> products, IRepository<Order> orders)
    {
        ArgumentNullException.ThrowIfNull(customers);
        ArgumentNullException.ThrowIfNull(products);
        ArgumentNullException.ThrowIfNull(orders);

        var oleksandr = CreateCustomer("Oleksandr Klimach", "oleksandr@example.com", new Address("Poltava", "Shevchenka", "12", "36000"));
        var iryna = CreateCustomer("Iryna Bondar", "iryna@example.com", new Address("Kyiv", "Khreshchatyk", "22", "01001"));
        var maksym = CreateCustomer("Maksym Tkachenko", "maksym@example.com", new Address("Lviv", "Svobody", "5", "79000"));
        var olena = CreateCustomer("Olena Shevchuk", "olena@example.com", new Address("Kharkiv", "Sumska", "40", "61000"));
        var dmytro = CreateCustomer("Dmytro Kovalenko", "dmytro@example.com", address: null);
        var sofiia = CreateCustomer("Sofiia Moroz", "sofiia@example.com", new Address("Odesa", "Derybasivska", "8", "65000"));

        foreach (var customer in new[] { oleksandr, iryna, maksym, olena, dmytro, sofiia })
        {
            customers.Add(customer);
        }

        var laptop = new Product("Laptop", 42000m);
        var smartphone = new Product("Smartphone", 28000m);
        var tablet = new Product("Tablet", 16000m);
        var monitor = new Product("Monitor", 9500m);
        var headphones = new Product("Headphones", 3500m);
        var keyboard = new Product("Keyboard", 2200m);
        var webcam = new Product("Webcam", 1900m);
        var powerBank = new Product("Power Bank", 1200m);
        var mouse = new Product("Mouse", 800m);
        var cable = new Product("USB-C Cable", 350m);

        foreach (var product in new[] { laptop, smartphone, tablet, monitor, headphones, keyboard, webcam, powerBank, mouse, cable })
        {
            products.Add(product);
        }

        Order[] sampleOrders =
        [
            CreateOrder(oleksandr, OrderStatus.Delivered, (laptop, 1), (mouse, 1)),
            CreateOrder(oleksandr, OrderStatus.Delivered, (cable, 2)),
            CreateOrder(oleksandr, OrderStatus.Paid, (headphones, 1)),
            CreateOrder(oleksandr, OrderStatus.Created, (monitor, 1)),
            CreateOrder(iryna, OrderStatus.Delivered, (smartphone, 1), (powerBank, 1)),
            CreateOrder(iryna, OrderStatus.Shipped, (cable, 3)),
            CreateOrder(iryna, OrderStatus.Cancelled, (tablet, 1)),
            CreateOrder(iryna, OrderStatus.Delivered, (keyboard, 1), (mouse, 1)),
            CreateOrder(maksym, OrderStatus.Delivered, (monitor, 2), (keyboard, 1)),
            CreateOrder(maksym, OrderStatus.Delivered, (mouse, 2)),
            CreateOrder(maksym, OrderStatus.Created, (webcam, 1)),
            CreateOrder(olena, OrderStatus.Delivered, (tablet, 1)),
            CreateOrder(olena, OrderStatus.Paid, (mouse, 1), (cable, 1)),
            CreateOrder(olena, OrderStatus.Shipped, (powerBank, 2)),
            CreateOrder(dmytro, OrderStatus.Paid, (keyboard, 1)),
            CreateOrder(dmytro, OrderStatus.Paid, (cable, 1)),
            CreateOrder(dmytro, OrderStatus.Cancelled, (laptop, 1)),
            CreateOrder(sofiia, OrderStatus.Delivered, (webcam, 1), (headphones, 1)),
            CreateOrder(sofiia, OrderStatus.Shipped, (mouse, 3)),
            CreateOrder(sofiia, OrderStatus.Paid, (cable, 4))
        ];

        foreach (var order in sampleOrders)
        {
            orders.Add(order);
        }
    }

    private static Customer CreateCustomer(string name, string email, Address? address) =>
        new() { Name = name, Email = email, Address = address };

    private static Order CreateOrder(Customer customer, OrderStatus status, params (Product Product, int Quantity)[] items)
    {
        var order = new Order(customer);

        foreach (var (product, quantity) in items)
        {
            order.AddItem(product, quantity);
        }

        if (status == OrderStatus.Cancelled)
        {
            order.Cancel();
            return order;
        }

        if (status >= OrderStatus.Paid)
        {
            PaymentService.Pay(order, new CardPayment(CardNumber));
        }

        if (status >= OrderStatus.Shipped)
        {
            order.Ship();
        }

        if (status >= OrderStatus.Delivered)
        {
            order.Deliver();
        }

        return order;
    }
}
