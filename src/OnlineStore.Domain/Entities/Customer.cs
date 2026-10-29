using System.Net.Mail;
using OnlineStore.Domain.Common;
using OnlineStore.Domain.Exceptions;
using OnlineStore.Domain.ValueObjects;

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

    public Address? Address { get; init; }

    public string? PhoneNumber
    {
        get;
        init => field = value is null || IsValidPhoneNumber(value)
            ? value
            : throw new DomainException("Customer phone number is invalid.");
    }

    // International format: "+" followed by 10-15 digits, e.g. +380501234567.
    private static bool IsValidPhoneNumber(string phoneNumber) =>
        phoneNumber is ['+', .. var digits]
        && digits.Length is >= 10 and <= 15
        && digits.All(char.IsAsciiDigit);
}
