using Visiotech.Pokemon.Domain.Abstractions;

namespace Visiotech.Pokemon.Domain.Pokemons;

public sealed record Ability : ValueObject
{
    private Ability(Name name)
    {
        Name = name;
    }

    public Name Name { get; }

    public static Ability Create(string name) =>
        new(Name.Create(name));
}
