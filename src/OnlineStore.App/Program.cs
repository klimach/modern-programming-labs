using System.Text;
using OnlineStore.App;
using OnlineStore.App.Utils;
using OnlineStore.Application.Orders;
using OnlineStore.Application.Payments;
using OnlineStore.Domain.Entities;
using OnlineStore.Domain.Exceptions;
using OnlineStore.Infrastructure.Payments;

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

var customer = new Customer { Name = "Oleksandr Klimach", Email = "oleksandr@example.com" };

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
PrintOrder(transferOrder);

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

static void PrintOrder(Order order)
{
    Console.WriteLine();
    Console.WriteLine($"Order for {order.Customer.Name}: {order.Status}");

    foreach (var item in order.Items)
    {
        Console.WriteLine($"  {item.Product.Name} x{item.Quantity} = {item.Total:0.00} UAH");
    }

    Console.WriteLine($"  Total: {order.TotalAmount:0.00} UAH");
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
