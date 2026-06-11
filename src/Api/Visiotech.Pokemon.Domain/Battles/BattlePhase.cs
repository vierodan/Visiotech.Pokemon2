using Visiotech.Pokemon.Domain.Abstractions;

namespace Visiotech.Pokemon.Domain.Battles;

public sealed class BattlePhase
{
    private readonly List<BattlePhaseEffectiveness> _effectivenessBreakdown = [];

    private BattlePhase()
    {
        MoveName = null!;
    }

    private BattlePhase(
        Guid battleId,
        int sequenceNumber,
        Guid attackerMyPokemonId,
        Guid defenderMyPokemonId,
        Guid moveId,
        string moveName,
        int randomFactor,
        decimal totalEffectiveness,
        int damage,
        int attackerRemainingHealthPoints,
        int defenderRemainingHealthPoints)
    {
        BattleId = battleId;
        SequenceNumber = sequenceNumber;
        AttackerMyPokemonId = attackerMyPokemonId;
        DefenderMyPokemonId = defenderMyPokemonId;
        MoveId = moveId;
        MoveName = moveName;
        RandomFactor = randomFactor;
        TotalEffectiveness = totalEffectiveness;
        Damage = damage;
        AttackerRemainingHealthPoints = attackerRemainingHealthPoints;
        DefenderRemainingHealthPoints = defenderRemainingHealthPoints;
    }

    public Guid BattleId { get; private set; }
    public int SequenceNumber { get; private set; }
    public Guid AttackerMyPokemonId { get; private set; }
    public Guid DefenderMyPokemonId { get; private set; }
    public Guid MoveId { get; private set; }
    public string MoveName { get; private set; }
    public int RandomFactor { get; private set; }
    public decimal TotalEffectiveness { get; private set; }
    public int Damage { get; private set; }
    public int AttackerRemainingHealthPoints { get; private set; }
    public int DefenderRemainingHealthPoints { get; private set; }
    public IReadOnlyCollection<BattlePhaseEffectiveness> EffectivenessBreakdown => _effectivenessBreakdown.AsReadOnly();

    public static BattlePhase Create(
        Guid battleId,
        int sequenceNumber,
        Guid attackerMyPokemonId,
        Guid defenderMyPokemonId,
        Guid moveId,
        string moveName,
        int randomFactor,
        decimal totalEffectiveness,
        int damage,
        int attackerRemainingHealthPoints,
        int defenderRemainingHealthPoints,
        IReadOnlyCollection<BattlePhaseEffectiveness> effectivenessBreakdown)
    {
        if (battleId == Guid.Empty)
        {
            throw new DomainException("Battle id cannot be empty.");
        }

        if (sequenceNumber <= 0)
        {
            throw new DomainException("Battle phase sequence number must be greater than 0.");
        }

        if (attackerMyPokemonId == Guid.Empty)
        {
            throw new DomainException("Battle phase attacker my pokemon id cannot be empty.");
        }

        if (defenderMyPokemonId == Guid.Empty)
        {
            throw new DomainException("Battle phase defender my pokemon id cannot be empty.");
        }

        if (attackerMyPokemonId == defenderMyPokemonId)
        {
            throw new DomainException("Battle phase attacker and defender must be different.");
        }

        if (moveId == Guid.Empty)
        {
            throw new DomainException("Battle phase move id cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(moveName))
        {
            throw new DomainException("Battle phase move name is required.");
        }

        if (randomFactor is < 85 or > 100)
        {
            throw new DomainException("Battle phase random factor must be between 85 and 100.");
        }

        if (damage < 0)
        {
            throw new DomainException("Battle phase damage cannot be negative.");
        }

        if (attackerRemainingHealthPoints < 0)
        {
            throw new DomainException("Battle phase attacker remaining health points cannot be negative.");
        }

        if (defenderRemainingHealthPoints < 0)
        {
            throw new DomainException("Battle phase defender remaining health points cannot be negative.");
        }

        ArgumentNullException.ThrowIfNull(effectivenessBreakdown);

        var phase = new BattlePhase(
            battleId,
            sequenceNumber,
            attackerMyPokemonId,
            defenderMyPokemonId,
            moveId,
            moveName.Trim(),
            randomFactor,
            totalEffectiveness,
            damage,
            attackerRemainingHealthPoints,
            defenderRemainingHealthPoints);

        foreach (var item in effectivenessBreakdown)
        {
            phase._effectivenessBreakdown.Add(item);
        }

        return phase;
    }
}
