using Visiotech.Pokemon.Domain.Abstractions;

namespace Visiotech.Pokemon.Domain.Pokemons;

public sealed record Name : ValueObject
{
    private Name(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Name Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Pokemon name is required.");
        }

        var normalized = value.Trim();
        if (normalized.Length > 100)
        {
            throw new DomainException("Pokemon name cannot exceed 100 characters.");
        }

        return new Name(normalized);
    }

    public override string ToString() => Value;
}
