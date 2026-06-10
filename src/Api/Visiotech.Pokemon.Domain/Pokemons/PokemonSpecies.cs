using Visiotech.Pokemon.Domain.Abstractions;

namespace Visiotech.Pokemon.Domain.Pokemons;

public sealed class PokemonSpecies : AggregateRoot<Guid>
{
    private readonly List<PokemonLearnableMove> _learnableMoves = [];

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
    public IReadOnlyCollection<PokemonLearnableMove> LearnableMoves => _learnableMoves.AsReadOnly();

    public IReadOnlyCollection<PokemonType> Types => Typing.Types;
    public IReadOnlyCollection<Guid> LearnableMoveIds => _learnableMoves.Select(learnableMove => learnableMove.PokemonMoveId).ToArray();

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

    public void AddLearnableMove(Guid pokemonMoveId)
    {
        if (pokemonMoveId == Guid.Empty)
        {
            throw new DomainException("Pokemon move id cannot be empty.");
        }

        if (_learnableMoves.Any(learnableMove => learnableMove.PokemonMoveId == pokemonMoveId))
        {
            throw new DomainException("Pokemon species cannot contain duplicate learnable moves.");
        }

        _learnableMoves.Add(PokemonLearnableMove.Create(Id, pokemonMoveId));
    }

    public void RemoveLearnableMove(Guid pokemonMoveId)
    {
        if (pokemonMoveId == Guid.Empty)
        {
            throw new DomainException("Pokemon move id cannot be empty.");
        }

        var learnableMove = _learnableMoves.SingleOrDefault(item => item.PokemonMoveId == pokemonMoveId);
        if (learnableMove is null)
        {
            throw new DomainException("Pokemon species does not contain the requested learnable move.");
        }

        _learnableMoves.Remove(learnableMove);
    }
}
