using System.Net.Mail;
using OnlineStore.Domain.Common;
using OnlineStore.Domain.Exceptions;

namespace OnlineStore.Domain.Entities;

public sealed class Customer : Entity
{
    public string FullName { get; }
    public string Email { get; }

    public Customer(string fullName, string email)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new DomainException("Customer name cannot be empty.");
        }

        if (!MailAddress.TryCreate(email, out _))
        {
            throw new DomainException("Customer email is invalid.");
        }

        FullName = fullName;
        Email = email;
    }
}
