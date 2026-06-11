using Visiotech.Pokemon.Domain.Abstractions;

namespace Visiotech.Pokemon.Domain.Pokemons;

public sealed record DamageCalculationInput
{
    public DamageCalculationInput(
        int attackerLevel,
        BaseStats attackerStats,
        BaseStats defenderStats,
        int defenderCurrentHealthPoints,
        PokemonType moveType,
        MoveCategory moveCategory,
        int movePower,
        IReadOnlyCollection<PokemonType> defenderTypes,
        int randomFactor)
    {
        if (attackerLevel <= 0)
        {
            throw new DomainException("Attacker level must be greater than 0.");
        }

        ArgumentNullException.ThrowIfNull(attackerStats);
        ArgumentNullException.ThrowIfNull(defenderStats);
        ArgumentNullException.ThrowIfNull(defenderTypes);

        if (defenderCurrentHealthPoints <= 0)
        {
            throw new DomainException("Defender current health points must be greater than 0.");
        }

        if (moveCategory is MoveCategory.Status)
        {
            throw new DomainException("Status moves cannot be used to calculate damage.");
        }

        if (movePower <= 0)
        {
            throw new DomainException("Move power must be greater than 0.");
        }

        var normalizedDefenderTypes = defenderTypes.ToArray();
        if (normalizedDefenderTypes.Length is < 1 or > 2)
        {
            throw new DomainException("Defender must have between 1 and 2 types.");
        }

        if (randomFactor is < 85 or > 100)
        {
            throw new DomainException("Damage random factor must be between 85 and 100.");
        }

        AttackerLevel = attackerLevel;
        AttackerStats = attackerStats;
        DefenderStats = defenderStats;
        DefenderCurrentHealthPoints = defenderCurrentHealthPoints;
        MoveType = moveType;
        MoveCategory = moveCategory;
        MovePower = movePower;
        DefenderTypes = normalizedDefenderTypes;
        RandomFactor = randomFactor;
    }

    public int AttackerLevel { get; }
    public BaseStats AttackerStats { get; }
    public BaseStats DefenderStats { get; }
    public int DefenderCurrentHealthPoints { get; }
    public PokemonType MoveType { get; }
    public MoveCategory MoveCategory { get; }
    public int MovePower { get; }
    public IReadOnlyCollection<PokemonType> DefenderTypes { get; }
    public int RandomFactor { get; }
}
