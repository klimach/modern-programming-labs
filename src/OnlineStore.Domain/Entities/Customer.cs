using System.Net.Mail;
using OnlineStore.Domain.Common;
using OnlineStore.Domain.Exceptions;

namespace OnlineStore.Domain.Entities;

public sealed class Customer : Entity
{
    public required string Name
    {
        get;
        init => field = !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new DomainException("Customer name cannot be empty.");
    }

    public required string Email
    {
        get;
        init => field = MailAddress.TryCreate(value, out _)
            ? value
            : throw new DomainException("Customer email is invalid.");
    }
}
