namespace OnlineStore.Domain.Entities;

public sealed class Customer
{
    public Guid Id { get; } = Guid.CreateVersion7();
    public string FullName { get; }
    public string Email { get; }

    public Customer(string fullName, string email)
    {
        FullName = fullName;
        Email = email;
    }
}
