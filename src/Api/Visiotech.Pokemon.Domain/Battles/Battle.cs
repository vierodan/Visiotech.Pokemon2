using Visiotech.Pokemon.Domain.Abstractions;

namespace Visiotech.Pokemon.Domain.Battles;

public sealed class Battle : AggregateRoot<Guid>
{
    private readonly List<BattleCombatant> _combatants = [];
    private readonly List<BattlePhase> _phases = [];

    private Battle()
    {
    }

    private Battle(Guid id)
        : base(id)
    {
    }

    public BattleStatus Status { get; private set; }
    public int CurrentTurnNumber { get; private set; }
    public Guid? NextAttackerMyPokemonId { get; private set; }
    public Guid? WinnerMyPokemonId { get; private set; }
    public Guid? LoserMyPokemonId { get; private set; }
    public IReadOnlyCollection<BattleCombatant> Combatants => _combatants.AsReadOnly();
    public IReadOnlyCollection<BattlePhase> Phases => _phases.AsReadOnly();

    public static Battle Create(
        Guid id,
        Guid firstMyPokemonId,
        int firstCurrentHealthPoints,
        int firstTotalHealthPoints,
        Guid secondMyPokemonId,
        int secondCurrentHealthPoints,
        int secondTotalHealthPoints)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Battle id cannot be empty.");
        }

        if (firstMyPokemonId == Guid.Empty)
        {
            throw new DomainException("First my pokemon id cannot be empty.");
        }

        if (secondMyPokemonId == Guid.Empty)
        {
            throw new DomainException("Second my pokemon id cannot be empty.");
        }

        if (firstMyPokemonId == secondMyPokemonId)
        {
            throw new DomainException("Battle must be created with two distinct my pokemon ids.");
        }

        if (firstCurrentHealthPoints <= 0)
        {
            throw new DomainException("First my pokemon must have current health points greater than 0 to start a battle.");
        }

        if (secondCurrentHealthPoints <= 0)
        {
            throw new DomainException("Second my pokemon must have current health points greater than 0 to start a battle.");
        }

        var battle = new Battle(id)
        {
            Status = BattleStatus.Created,
            CurrentTurnNumber = 1,
            NextAttackerMyPokemonId = firstMyPokemonId
        };

        battle._combatants.Add(BattleCombatant.Create(
            battle.Id,
            1,
            firstMyPokemonId,
            firstCurrentHealthPoints,
            firstTotalHealthPoints));
        battle._combatants.Add(BattleCombatant.Create(
            battle.Id,
            2,
            secondMyPokemonId,
            secondCurrentHealthPoints,
            secondTotalHealthPoints));

        return battle;
    }

    public void RecordPhase(BattlePhaseRegistration registration)
    {
        ArgumentNullException.ThrowIfNull(registration);

        if (Status == BattleStatus.Finished)
        {
            throw new DomainException("Finished battles cannot record additional phases.");
        }

        if (registration.SequenceNumber != CurrentTurnNumber)
        {
            throw new DomainException("Battle phase sequence number must match the current turn number.");
        }

        if (registration.AttackerMyPokemonId != NextAttackerMyPokemonId)
        {
            throw new DomainException("Battle phase attacker must match the next attacker configured for the battle.");
        }

        var attacker = _combatants.SingleOrDefault(combatant => combatant.MyPokemonId == registration.AttackerMyPokemonId)
            ?? throw new DomainException("Battle phase attacker does not belong to the battle.");

        var defender = _combatants.SingleOrDefault(combatant => combatant.MyPokemonId == registration.DefenderMyPokemonId)
            ?? throw new DomainException("Battle phase defender does not belong to the battle.");

        var effectivenessBreakdown = registration.EffectivenessBreakdown
            .Select(item => BattlePhaseEffectiveness.Create(
                Id,
                registration.SequenceNumber,
                item.DefenderType,
                item.Multiplier))
            .ToArray();

        var phase = BattlePhase.Create(
            Id,
            registration.SequenceNumber,
            registration.AttackerMyPokemonId,
            registration.DefenderMyPokemonId,
            registration.MoveId,
            registration.MoveName,
            registration.RandomFactor,
            registration.TotalEffectiveness,
            registration.Damage,
            registration.AttackerRemainingHealthPoints,
            registration.DefenderRemainingHealthPoints,
            effectivenessBreakdown);

        attacker.UpdateCurrentHealthPoints(registration.AttackerRemainingHealthPoints);
        defender.UpdateCurrentHealthPoints(registration.DefenderRemainingHealthPoints);

        _phases.Add(phase);

        if (registration.DefenderRemainingHealthPoints == 0 && !registration.FinishesBattle)
        {
            throw new DomainException("Battle must finish when a defender reaches 0 current health points.");
        }

        if (registration.FinishesBattle)
        {
            if (registration.DefenderRemainingHealthPoints != 0)
            {
                throw new DomainException("Finished battles require the defender to have 0 current health points.");
            }

            Status = BattleStatus.Finished;
            CurrentTurnNumber = registration.SequenceNumber;
            NextAttackerMyPokemonId = null;
            WinnerMyPokemonId = attacker.MyPokemonId;
            LoserMyPokemonId = defender.MyPokemonId;
            return;
        }

        if (registration.NextAttackerMyPokemonId is null)
        {
            throw new DomainException("Next attacker my pokemon id is required when the battle continues.");
        }

        if (_combatants.All(combatant => combatant.MyPokemonId != registration.NextAttackerMyPokemonId.Value))
        {
            throw new DomainException("Next attacker my pokemon id does not belong to the battle.");
        }

        Status = BattleStatus.InProgress;
        CurrentTurnNumber = registration.SequenceNumber + 1;
        NextAttackerMyPokemonId = registration.NextAttackerMyPokemonId.Value;
        WinnerMyPokemonId = null;
        LoserMyPokemonId = null;
    }
}
