using Visiotech.Pokemon.Domain.Abstractions;

namespace Visiotech.Pokemon.Domain.Pokemons;

public sealed record Level : ValueObject
{
    private Level(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static Level Create(int value)
    {
        if (value is < 1 or > 100)
        {
            throw new DomainException("Pokemon level must be between 1 and 100.");
        }

        return new Level(value);
    }

    public override string ToString() => Value.ToString();
}
