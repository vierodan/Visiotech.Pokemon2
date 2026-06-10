using Visiotech.Pokemon.Domain.Abstractions;

namespace Visiotech.Pokemon.Domain.Pokemons;

public sealed record Name : ValueObject
{
    private Name()
    {
        Value = string.Empty;
        NormalizedValue = string.Empty;
    }

    private Name(string value, string normalizedValue)
    {
        Value = value;
        NormalizedValue = normalizedValue;
    }

    public string Value { get; private set; }
    public string NormalizedValue { get; private set; }

    public static Name Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Name is required.");
        }

        var normalized = value.Trim();
        if (normalized.Length > 100)
        {
            throw new DomainException("Name cannot exceed 100 characters.");
        }

        return new Name(normalized, Normalize(normalized));
    }

    public override string ToString() => Value;

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();
}
