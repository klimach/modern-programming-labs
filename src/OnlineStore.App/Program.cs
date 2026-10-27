using System.Text;
using OnlineStore.App;
using OnlineStore.App.Data;
using OnlineStore.App.Utils;
using OnlineStore.Application.Orders;
using OnlineStore.Application.Payments;
using OnlineStore.Application.Statistics;
using OnlineStore.Domain.Entities;
using OnlineStore.Domain.Exceptions;
using OnlineStore.Domain.ValueObjects;
using OnlineStore.Infrastructure.Payments;
using OnlineStore.Infrastructure.Repositories;

const string CardNumber = "4111111111111111";
const string Iban = "UA213223130000026007233566001";

var output = new StringBuilder()
    .AppendLine("Modern Programming Course")
    .AppendLine()
    .AppendLine($"Student: {StudentInfo.FullName}")
    .AppendLine($"Group: {StudentInfo.Group}")
    .AppendLine()
    .AppendLine($"Variant: {StudentInfo.Variant}")
    .AppendLine($"Domain: {StudentInfo.Domain}")
    .AppendLine()
    .AppendLine($".NET: {FrameworkUtils.GetDotNetVersion()}");

Console.Write(output);

var paymentService = new OrderPaymentService();

var customer = new Customer
{
    Name = "Oleksandr Klimach",
    Email = "oleksandr@example.com",
    Address = new Address("Poltava", "Shevchenka", "12", "36000"),
    PhoneNumber = "+380501234567"
};

var guest = new Customer { Name = "Guest Customer", Email = "guest@example.com" };

PrintCustomer(customer);
PrintCustomer(guest);

var laptop = new Product("Laptop", 45000m);
var mouse = new Product("Mouse", 800m);
var headphones = new Product("Headphones", 3500m);

var cardOrder = new Order(customer);
cardOrder.AddItem(laptop, 1);
cardOrder.AddItem(mouse, 1);
cardOrder.AddItem(mouse, 1);

var transferOrder = new Order(customer);
transferOrder.AddItem(headphones, 1);
transferOrder.ChangeItemQuantity(headphones.Id, 2);
transferOrder.AddItem(mouse, 1);
transferOrder.RemoveItem(mouse.Id);

IPaymentMethod paymentMethod = new CardPayment(CardNumber);
paymentService.Pay(cardOrder, paymentMethod);
PrintOrder(cardOrder);

paymentMethod = new BankTransferPayment(Iban);
paymentService.Pay(transferOrder, paymentMethod);
transferOrder.Ship();
transferOrder.Deliver();
PrintOrder(transferOrder);

var smallOrder = new Order(guest);
smallOrder.AddItem(mouse, 1);
paymentService.Pay(smallOrder, new CardPayment(CardNumber));
PrintOrder(smallOrder);

laptop.ChangePrice(42000m);
var paidLaptop = cardOrder.Items.First(item => item.Product == laptop);

Console.WriteLine();
Console.WriteLine($"Laptop price changed to {laptop.Price:0.00} UAH, paid order keeps {paidLaptop.UnitPrice:0.00} UAH");

var cancelledOrder = new Order(customer);
cancelledOrder.AddItem(mouse, 1);
cancelledOrder.Cancel();

Console.WriteLine();
Console.WriteLine("Validation:");
TryRun("Pay the order twice", () => paymentService.Pay(cardOrder, new CardPayment(CardNumber)));
TryRun("Pay a cancelled order", () => paymentService.Pay(cancelledOrder, new BankTransferPayment(Iban)));
TryRun("Add item to a paid order", () => cardOrder.AddItem(headphones, 1));
TryRun("Create product with price 0", () => new Product("Gift card", 0m));
TryRun("Change product price to negative", () => laptop.ChangePrice(-1m));
TryRun("Add item with quantity 0", () => new Order(customer).AddItem(mouse, 0));
TryRun("Change quantity in a paid order", () => cardOrder.ChangeItemQuantity(mouse.Id, 5));
TryRun("Use invalid card number", () => new CardPayment("4111111111111112"));
TryRun("Ship order without delivery address", smallOrder.Ship);
TryRun("Deliver order that is not shipped", smallOrder.Deliver);
TryRun("Cancel delivered order", transferOrder.Cancel);
TryRun("Create address with invalid postal code", () => new Address("Poltava", "Shevchenka", "12", "3600"));
TryRun("Create customer with invalid phone", () => new Customer { Name = "Test", Email = "test@example.com", PhoneNumber = "0501234567" });

var customerRepository = new InMemoryRepository<Customer>();
var productRepository = new InMemoryRepository<Product>();
var orderRepository = new InMemoryRepository<Order>();

SampleData.Seed(customerRepository, productRepository, orderRepository);

var statistics = new OrderStatisticsService(orderRepository);

Console.WriteLine();
Console.WriteLine(
    $"Sample data: {customerRepository.GetAll().Count} customers, " +
    $"{productRepository.GetAll().Count} products, {orderRepository.GetAll().Count} orders");

var largeOrders = statistics.GetOrdersWithTotalAbove(1000m);
Console.WriteLine();
Console.WriteLine($"Orders with total > 1000 UAH: {largeOrders.Count}");

Console.WriteLine();
Console.WriteLine("Top 5 most expensive orders:");
foreach (var order in statistics.GetMostExpensiveOrders(5))
{
    Console.WriteLine($"  {order.TotalAmount,10:0.00} UAH  {order.Customer.Name} ({order.Status})");
}

Console.WriteLine();
Console.WriteLine($"Total sales: {statistics.GetTotalSales():0.00} UAH");
Console.WriteLine($"Average order value: {statistics.GetAverageOrderValue():0.00} UAH");

Console.WriteLine();
Console.WriteLine("Orders per customer:");
foreach (var (buyer, orderCount) in statistics.GetOrderCountByCustomer())
{
    Console.WriteLine($"  {buyer.Name}: {orderCount}");
}

Console.WriteLine();
Console.WriteLine($"Ordered products: {string.Join(", ", statistics.GetOrderedProducts().Select(product => product.Name))}");

Console.WriteLine();
Console.WriteLine("Top 3 products by sold quantity:");
foreach (var (product, quantitySold) in statistics.GetBestSellingProducts(3))
{
    Console.WriteLine($"  {product.Name}: {quantitySold}");
}

Console.WriteLine();
Console.WriteLine("Customers who spent more than 5000 UAH:");
foreach (var (buyer, totalSpent) in statistics.GetCustomersWithSpendingAbove(5000m))
{
    Console.WriteLine($"  {buyer.Name}: {totalSpent:0.00} UAH");
}

Console.WriteLine();
Console.WriteLine($"Any order > 10000 UAH: {(statistics.HasOrderWithTotalAbove(10000m) ? "yes" : "no")}");

Console.WriteLine();
Console.WriteLine("Spending by customer id:");
foreach (var (customerId, totalSpent) in statistics.GetSpendingByCustomerId())
{
    Console.WriteLine($"  {customerId} ({customerRepository.GetById(customerId)?.Name}): {totalSpent:0.00} UAH");
}

static void PrintCustomer(Customer customer)
{
    Console.WriteLine();
    Console.WriteLine($"Customer: {customer.Name} <{customer.Email}>");
    Console.WriteLine($"  Phone: {customer.PhoneNumber ?? "not specified"}");
    Console.WriteLine($"  Address: {customer.Address?.ToString() ?? "not specified"}");
}

static void PrintOrder(Order order)
{
    Console.WriteLine();
    Console.WriteLine($"Order for {order.Customer.Name}: {order.Status} - {order.GetStatusDescription()}");

    foreach (var item in order.Items)
    {
        Console.WriteLine($"  {item.Product.Name} x{item.Quantity} = {item.Total:0.00} UAH");
    }

    Console.WriteLine($"  Total: {order.TotalAmount:0.00} UAH ({order.GetOrderCategory()})");
    Console.WriteLine($"  Free delivery: {(order.HasFreeDelivery ? "yes" : "no")}");
    Console.WriteLine($"  Paid with: {order.Payment?.PaymentMethod}");
    Console.WriteLine($"  Transaction: {order.Payment?.TransactionId}");
}

static void TryRun(string description, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  {description}: OK");
    }
    catch (Exception exception) when (exception is DomainException or ArgumentException)
    {
        Console.WriteLine($"  {description}: {exception.Message}");
    }
}
