using Visiotech.Pokemon.Domain.Abstractions;

namespace Visiotech.Pokemon.Domain.Pokemons;

public sealed record Move : ValueObject
{
    private Move(Name name, PokemonType type, int power)
    {
        Name = name;
        Type = type;
        Power = power;
    }

    public Name Name { get; }
    public PokemonType Type { get; }
    public int Power { get; }

    public static Move Create(string name, PokemonType type, int power)
    {
        if (power <= 0)
        {
            throw new DomainException("Move power must be greater than zero.");
        }

        return new Move(Name.Create(name), type, power);
    }
}
