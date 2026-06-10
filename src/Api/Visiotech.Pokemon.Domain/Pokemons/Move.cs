using Visiotech.Pokemon.Domain.Abstractions;

namespace Visiotech.Pokemon.Domain.Pokemons;

public sealed record Move : ValueObject
{
    private Move(Name name, PokemonType type, MoveCategory category, int power)
    {
        Name = name;
        Type = type;
        Category = category;
        Power = power;
    }

    public Name Name { get; }
    public PokemonType Type { get; }
    public MoveCategory Category { get; }
    public int Power { get; }

    public static Move Create(string name, PokemonType type, MoveCategory category, int power) =>
        Create(Name.Create(name), type, category, power);

    public static Move Create(Name name, PokemonType type, MoveCategory category, int power)
    {
        if (category is MoveCategory.Status && power != 0)
        {
            throw new DomainException("Status move power must be 0.");
        }

        if (category is not MoveCategory.Status && power <= 0)
        {
            throw new DomainException("Move power must be greater than 0 for Physical or Special categories.");
        }

        return new Move(name, type, category, power);
    }
}
