using OnlineStore.Domain.Entities;
using OnlineStore.Domain.Enums;
using OnlineStore.Domain.Exceptions;
using OnlineStore.Domain.ValueObjects;

namespace OnlineStore.Domain.Tests.Entities;

public class OrderTests
{
    private readonly Customer _customer = new()
    {
        Name = "Oleksandr Klimach",
        Email = "oleksandr@example.com",
        Address = new Address("Poltava", "Shevchenka", "12", "36000")
    };
    private readonly Product _laptop = new("Laptop", 45000m);
    private readonly Product _mouse = new("Mouse", 800m);

    [Fact]
    public void Constructor_CreatesEmptyOrderWithCreatedStatus()
    {
        var order = new Order(_customer);

        Assert.Equal(OrderStatus.Created, order.Status);
        Assert.Empty(order.Items);
        Assert.Equal(0m, order.TotalAmount);
        Assert.Null(order.Payment);
    }

    [Fact]
    public void AddItem_WithNewProduct_AddsItemAndUpdatesTotal()
    {
        var order = new Order(_customer);

        order.AddItem(_laptop, 1);
        order.AddItem(_mouse, 2);

        Assert.Equal(2, order.Items.Count);
        Assert.Equal(46600m, order.TotalAmount);
    }

    [Fact]
    public void AddItem_WithSameProduct_IncreasesQuantity()
    {
        var order = new Order(_customer);

        order.AddItem(_mouse, 1);
        order.AddItem(_mouse, 2);

        var item = Assert.Single(order.Items);
        Assert.Equal(3, item.Quantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddItem_WithNonPositiveQuantity_ThrowsDomainException(int quantity)
    {
        var order = new Order(_customer);

        Assert.Throws<DomainException>(() => order.AddItem(_mouse, quantity));
        Assert.Empty(order.Items);
    }

    [Fact]
    public void AddItem_KeepsPriceAtTheMomentOfAdding()
    {
        var order = new Order(_customer);
        order.AddItem(_laptop, 1);

        _laptop.ChangePrice(42000m);

        var item = Assert.Single(order.Items);
        Assert.Equal(45000m, item.UnitPrice);
        Assert.Equal(45000m, order.TotalAmount);
    }

    [Fact]
    public void RemoveItem_WithExistingProduct_RemovesItem()
    {
        var order = new Order(_customer);
        order.AddItem(_laptop, 1);
        order.AddItem(_mouse, 1);

        order.RemoveItem(_mouse.Id);

        var item = Assert.Single(order.Items);
        Assert.Equal(_laptop, item.Product);
    }

    [Fact]
    public void RemoveItem_WithMissingProduct_ThrowsDomainException()
    {
        var order = new Order(_customer);

        Assert.Throws<DomainException>(() => order.RemoveItem(_mouse.Id));
    }

    [Fact]
    public void ChangeItemQuantity_WithPositiveQuantity_UpdatesQuantity()
    {
        var order = new Order(_customer);
        order.AddItem(_mouse, 1);

        order.ChangeItemQuantity(_mouse.Id, 5);

        Assert.Equal(5, Assert.Single(order.Items).Quantity);
        Assert.Equal(4000m, order.TotalAmount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ChangeItemQuantity_WithNonPositiveQuantity_ThrowsDomainException(int quantity)
    {
        var order = new Order(_customer);
        order.AddItem(_mouse, 1);

        Assert.Throws<DomainException>(() => order.ChangeItemQuantity(_mouse.Id, quantity));
        Assert.Equal(1, Assert.Single(order.Items).Quantity);
    }

    [Fact]
    public void Items_CannotBeModifiedDirectly()
    {
        var order = new Order(_customer);
        order.AddItem(_mouse, 1);

        var items = Assert.IsAssignableFrom<ICollection<OrderItem>>(order.Items);

        Assert.IsNotType<List<OrderItem>>(order.Items);
        Assert.Throws<NotSupportedException>(() => items.Clear());
        Assert.Single(order.Items);
    }

    [Fact]
    public void Pay_WithMatchingReceipt_MarksOrderAsPaid()
    {
        var order = CreateOrderWithMouse();
        var receipt = CreateReceipt(order.TotalAmount);

        order.Pay(receipt);

        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.Same(receipt, order.Payment);
    }

    [Fact]
    public void Pay_WhenAlreadyPaid_ThrowsDomainException()
    {
        var order = CreateOrderWithMouse();
        order.Pay(CreateReceipt(order.TotalAmount));

        Assert.Throws<DomainException>(() => order.Pay(CreateReceipt(order.TotalAmount)));
    }

    [Fact]
    public void Pay_WhenCancelled_ThrowsDomainException()
    {
        var order = CreateOrderWithMouse();
        order.Cancel();

        Assert.Throws<DomainException>(() => order.Pay(CreateReceipt(order.TotalAmount)));
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void Pay_WhenEmpty_ThrowsDomainException()
    {
        var order = new Order(_customer);

        Assert.Throws<DomainException>(() => order.Pay(CreateReceipt(100m)));
    }

    [Fact]
    public void Pay_WithAmountDifferentFromTotal_ThrowsDomainException()
    {
        var order = CreateOrderWithMouse();

        Assert.Throws<DomainException>(() => order.Pay(CreateReceipt(order.TotalAmount - 1)));
        Assert.Equal(OrderStatus.Created, order.Status);
    }

    [Fact]
    public void ModifyingItems_WhenPaid_ThrowsDomainException()
    {
        var order = CreateOrderWithMouse();
        order.Pay(CreateReceipt(order.TotalAmount));

        Assert.Throws<DomainException>(() => order.AddItem(_laptop, 1));
        Assert.Throws<DomainException>(() => order.RemoveItem(_mouse.Id));
        Assert.Throws<DomainException>(() => order.ChangeItemQuantity(_mouse.Id, 2));
    }

    [Fact]
    public void Cancel_WhenCreated_MarksOrderAsCancelled()
    {
        var order = CreateOrderWithMouse();

        order.Cancel();

        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void Cancel_WhenPaid_ThrowsDomainException()
    {
        var order = CreateOrderWithMouse();
        order.Pay(CreateReceipt(order.TotalAmount));

        Assert.Throws<DomainException>(order.Cancel);
        Assert.Equal(OrderStatus.Paid, order.Status);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ThrowsDomainException()
    {
        var order = CreateOrderWithMouse();
        order.Cancel();

        Assert.Throws<DomainException>(order.Cancel);
    }

    [Fact]
    public void Ship_WhenPaid_MarksOrderAsShipped()
    {
        var order = CreatePaidOrder();

        order.Ship();

        Assert.Equal(OrderStatus.Shipped, order.Status);
    }

    [Fact]
    public void Ship_WhenNotPaid_ThrowsDomainException()
    {
        var order = CreateOrderWithMouse();

        Assert.Throws<DomainException>(order.Ship);
        Assert.Equal(OrderStatus.Created, order.Status);
    }

    [Fact]
    public void Ship_WhenCustomerHasNoAddress_ThrowsDomainException()
    {
        var customer = new Customer { Name = "Oleksandr Klimach", Email = "oleksandr@example.com" };
        var order = new Order(customer);
        order.AddItem(_mouse, 1);
        order.Pay(CreateReceipt(order.TotalAmount));

        Assert.Throws<DomainException>(order.Ship);
        Assert.Equal(OrderStatus.Paid, order.Status);
    }

    [Fact]
    public void Deliver_WhenShipped_MarksOrderAsDelivered()
    {
        var order = CreatePaidOrder();
        order.Ship();

        order.Deliver();

        Assert.Equal(OrderStatus.Delivered, order.Status);
    }

    [Fact]
    public void Deliver_WhenNotShipped_ThrowsDomainException()
    {
        var order = CreatePaidOrder();

        Assert.Throws<DomainException>(order.Deliver);
        Assert.Equal(OrderStatus.Paid, order.Status);
    }

    [Fact]
    public void Pay_WhenShipped_ThrowsDomainException()
    {
        var order = CreatePaidOrder();
        order.Ship();

        Assert.Throws<DomainException>(() => order.Pay(CreateReceipt(order.TotalAmount)));
    }

    [Fact]
    public void Cancel_WhenShippedOrDelivered_ThrowsDomainException()
    {
        var shipped = CreatePaidOrder();
        shipped.Ship();
        var delivered = CreatePaidOrder();
        delivered.Ship();
        delivered.Deliver();

        Assert.Throws<DomainException>(shipped.Cancel);
        Assert.Throws<DomainException>(delivered.Cancel);
    }

    private Order CreateOrderWithMouse()
    {
        var order = new Order(_customer);
        order.AddItem(_mouse, 1);

        return order;
    }

    private Order CreatePaidOrder()
    {
        var order = CreateOrderWithMouse();
        order.Pay(CreateReceipt(order.TotalAmount));

        return order;
    }

    private static PaymentReceipt CreateReceipt(decimal amount) =>
        new("TEST-TRANSACTION", "Test payment", amount);
}
