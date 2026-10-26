using OnlineStore.Domain.Exceptions;

namespace OnlineStore.Domain.ValueObjects;

public sealed record Address(string City, string Street, string Building, string PostalCode)
{
    private const int PostalCodeLength = 5;

    public string City { get; init; } = !string.IsNullOrWhiteSpace(City)
        ? City
        : throw new DomainException("City cannot be empty.");

    public string Street { get; init; } = !string.IsNullOrWhiteSpace(Street)
        ? Street
        : throw new DomainException("Street cannot be empty.");

    public string Building { get; init; } = !string.IsNullOrWhiteSpace(Building)
        ? Building
        : throw new DomainException("Building cannot be empty.");

    public string PostalCode { get; init; } = PostalCode is { Length: PostalCodeLength } && PostalCode.All(char.IsAsciiDigit)
        ? PostalCode
        : throw new DomainException($"Postal code must contain {PostalCodeLength} digits.");

    public override string ToString() => $"{Street} {Building}, {City}, {PostalCode}";
}
