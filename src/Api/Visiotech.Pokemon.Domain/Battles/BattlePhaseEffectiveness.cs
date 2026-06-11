using Visiotech.Pokemon.Domain.Abstractions;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Domain.Battles;

public sealed class BattlePhaseEffectiveness
{
    private BattlePhaseEffectiveness()
    {
    }

    private BattlePhaseEffectiveness(
        Guid battleId,
        int battlePhaseSequenceNumber,
        PokemonType defenderType,
        decimal multiplier)
    {
        BattleId = battleId;
        BattlePhaseSequenceNumber = battlePhaseSequenceNumber;
        DefenderType = defenderType;
        Multiplier = multiplier;
    }

    public Guid BattleId { get; private set; }
    public int BattlePhaseSequenceNumber { get; private set; }
    public PokemonType DefenderType { get; private set; }
    public decimal Multiplier { get; private set; }

    public static BattlePhaseEffectiveness Create(
        Guid battleId,
        int battlePhaseSequenceNumber,
        PokemonType defenderType,
        decimal multiplier)
    {
        if (battleId == Guid.Empty)
        {
            throw new DomainException("Battle id cannot be empty.");
        }

        if (battlePhaseSequenceNumber <= 0)
        {
            throw new DomainException("Battle phase sequence number must be greater than 0.");
        }

        if (multiplier < 0m)
        {
            throw new DomainException("Battle phase effectiveness multiplier cannot be negative.");
        }

        return new BattlePhaseEffectiveness(
            battleId,
            battlePhaseSequenceNumber,
            defenderType,
            multiplier);
    }
}
