namespace Visiotech.Pokemon.Domain.Pokemons;

public static class MoveDamageCalculator
{
    public static DamageCalculationResult Calculate(DamageCalculationInput input)
    {
        var (offensiveStat, offensiveStatValue, defensiveStat, defensiveStatValue) = SelectStats(input);

        var effectivenessBreakdown = input.DefenderTypes
            .Select(defenderType => new DamageCalculationEffectiveness(
                defenderType,
                PokemonTypeEffectivenessChart.GetMultiplier(input.MoveType, defenderType)))
            .ToArray();

        var totalEffectiveness = effectivenessBreakdown.Aggregate(1m, static (current, item) => current * item.Multiplier);
        var baseDamage = (((2m * input.AttackerLevel) / 5m + 2m) * offensiveStatValue * input.MovePower / defensiveStatValue) / 50m;

        var rawDamage = totalEffectiveness == 0m
            ? 0
            : Math.Max(0, decimal.ToInt32(decimal.Floor(baseDamage * totalEffectiveness * input.RandomFactor / 100m)));

        var damage = Math.Min(rawDamage, input.DefenderCurrentHealthPoints);
        var defenderRemainingHealthPoints = Math.Max(0, input.DefenderCurrentHealthPoints - damage);

        return new DamageCalculationResult(
            offensiveStat,
            offensiveStatValue,
            defensiveStat,
            defensiveStatValue,
            input.RandomFactor,
            baseDamage,
            effectivenessBreakdown,
            totalEffectiveness,
            rawDamage,
            damage,
            defenderRemainingHealthPoints);
    }

    private static (string OffensiveStat, int OffensiveStatValue, string DefensiveStat, int DefensiveStatValue) SelectStats(
        DamageCalculationInput input) =>
        input.MoveCategory switch
        {
            MoveCategory.Physical => ("Attack", input.AttackerStats.Attack, "Defense", input.DefenderStats.Defense),
            MoveCategory.Special => ("SpecialAttack", input.AttackerStats.SpecialAttack, "SpecialDefense", input.DefenderStats.SpecialDefense),
            _ => throw new InvalidOperationException($"Move category '{input.MoveCategory}' is not supported for damage calculation.")
        };
}
