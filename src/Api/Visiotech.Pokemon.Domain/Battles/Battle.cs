using Visiotech.Pokemon.Domain.Abstractions;

namespace Visiotech.Pokemon.Domain.Battles;

public sealed class Battle : AggregateRoot<Guid>
{
    private readonly List<BattleCombatant> _combatants = [];

    private Battle()
    {
    }

    private Battle(Guid id)
        : base(id)
    {
    }

    public BattleStatus Status { get; private set; }
    public int CurrentTurnNumber { get; private set; }
    public Guid NextAttackerMyPokemonId { get; private set; }
    public IReadOnlyCollection<BattleCombatant> Combatants => _combatants.AsReadOnly();

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
}
