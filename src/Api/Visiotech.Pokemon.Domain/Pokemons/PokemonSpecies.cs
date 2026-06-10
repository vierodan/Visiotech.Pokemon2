using Visiotech.Pokemon.Domain.Abstractions;

namespace Visiotech.Pokemon.Domain.Pokemons;

public sealed class PokemonSpecies : AggregateRoot<Guid>
{
    private PokemonSpecies()
    {
        Name = null!;
        NormalizedName = string.Empty;
        Typing = null!;
        BaseStats = null!;
    }

    private PokemonSpecies(
        Guid id,
        Name name,
        PokemonTyping typing,
        BaseStats baseStats)
        : base(id)
    {
        Name = name;
        NormalizedName = name.NormalizedValue;
        Typing = typing;
        BaseStats = baseStats;
    }

    public Name Name { get; private set; }
    public string NormalizedName { get; private set; }
    public PokemonTyping Typing { get; private set; }
    public BaseStats BaseStats { get; private set; }

    public IReadOnlyCollection<PokemonType> Types => Typing.Types;

    public static PokemonSpecies Create(
        Guid id,
        Name name,
        PokemonTyping typing,
        BaseStats baseStats)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Pokemon species id cannot be empty.");
        }

        return new PokemonSpecies(id, name, typing, baseStats);
    }

    public void Rename(Name name)
    {
        Name = name;
        NormalizedName = name.NormalizedValue;
    }

    public void ReconfigureTyping(PokemonTyping typing) => Typing = typing;

    public void ReconfigureBaseStats(BaseStats baseStats) => BaseStats = baseStats;
}
