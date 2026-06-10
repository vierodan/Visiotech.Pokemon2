using Visiotech.Pokemon.Domain.Abstractions;

namespace Visiotech.Pokemon.Domain.Pokemons;

public sealed class Pokemon : AggregateRoot<Guid>
{
    private readonly List<Move> _moves;
    private readonly List<Ability> _abilities;

    private Pokemon(
        Guid id,
        Name name,
        PokemonType type,
        Level level,
        BaseStats stats,
        IEnumerable<Move> moves,
        IEnumerable<Ability> abilities)
        : base(id)
    {
        Name = name;
        Type = type;
        Level = level;
        Stats = stats;
        _moves = NormalizeMoves(moves);
        _abilities = NormalizeAbilities(abilities);
    }

    public Name Name { get; private set; }
    public PokemonType Type { get; private set; }
    public Level Level { get; private set; }
    public BaseStats Stats { get; private set; }
    public IReadOnlyCollection<Move> Moves => _moves.AsReadOnly();
    public IReadOnlyCollection<Ability> Abilities => _abilities.AsReadOnly();

    public static Pokemon Create(
        Guid id,
        Name name,
        PokemonType type,
        Level level,
        BaseStats stats,
        IEnumerable<Move> moves,
        IEnumerable<Ability> abilities)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Pokemon id cannot be empty.");
        }

        return new Pokemon(id, name, type, level, stats, moves, abilities);
    }

    public void Rename(Name name) => Name = name;

    public void ChangeLevel(Level level) => Level = level;

    public void ReconfigureStats(BaseStats stats) => Stats = stats;

    public void ReplaceMoveSet(IEnumerable<Move> moves)
    {
        _moves.Clear();
        _moves.AddRange(NormalizeMoves(moves));
    }

    public void ReplaceAbilities(IEnumerable<Ability> abilities)
    {
        _abilities.Clear();
        _abilities.AddRange(NormalizeAbilities(abilities));
    }

    private static List<Move> NormalizeMoves(IEnumerable<Move> moves)
    {
        var normalized = moves.ToList();
        EnsureCollectionLimit(normalized.Count, "moves");

        if (normalized
            .Select(move => move.Name.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count() != normalized.Count)
        {
            throw new DomainException("Pokemon moves must be unique.");
        }

        return normalized;
    }

    private static List<Ability> NormalizeAbilities(IEnumerable<Ability> abilities)
    {
        var normalized = abilities.ToList();
        EnsureCollectionLimit(normalized.Count, "abilities");

        if (normalized
            .Select(ability => ability.Name.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count() != normalized.Count)
        {
            throw new DomainException("Pokemon abilities must be unique.");
        }

        return normalized;
    }

    private static void EnsureCollectionLimit(int count, string collectionName)
    {
        if (count > 4)
        {
            throw new DomainException($"Pokemon {collectionName} cannot contain more than 4 items.");
        }
    }
}
