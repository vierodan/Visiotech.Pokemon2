using Visiotech.Pokemon.Domain.Abstractions;

namespace Visiotech.Pokemon.Domain.Battles;

public sealed class BattleCombatant
{
    private BattleCombatant()
    {
    }

    private BattleCombatant(
        Guid battleId,
        int slotNumber,
        Guid myPokemonId,
        int currentHealthPoints,
        int totalHealthPoints)
    {
        BattleId = battleId;
        SlotNumber = slotNumber;
        MyPokemonId = myPokemonId;
        CurrentHealthPoints = currentHealthPoints;
        TotalHealthPoints = totalHealthPoints;
    }

    public Guid BattleId { get; private set; }
    public int SlotNumber { get; private set; }
    public Guid MyPokemonId { get; private set; }
    public int CurrentHealthPoints { get; private set; }
    public int TotalHealthPoints { get; private set; }

    public static BattleCombatant Create(
        Guid battleId,
        int slotNumber,
        Guid myPokemonId,
        int currentHealthPoints,
        int totalHealthPoints)
    {
        if (battleId == Guid.Empty)
        {
            throw new DomainException("Battle id cannot be empty.");
        }

        if (slotNumber is < 1 or > 2)
        {
            throw new DomainException("Battle slot number must be between 1 and 2.");
        }

        if (myPokemonId == Guid.Empty)
        {
            throw new DomainException("My pokemon id cannot be empty.");
        }

        if (currentHealthPoints < 0)
        {
            throw new DomainException("Battle combatant current health points cannot be negative.");
        }

        if (totalHealthPoints <= 0)
        {
            throw new DomainException("Battle combatant total health points must be greater than 0.");
        }

        if (currentHealthPoints > totalHealthPoints)
        {
            throw new DomainException("Battle combatant current health points cannot exceed total health points.");
        }

        return new BattleCombatant(
            battleId,
            slotNumber,
            myPokemonId,
            currentHealthPoints,
            totalHealthPoints);
    }
}
