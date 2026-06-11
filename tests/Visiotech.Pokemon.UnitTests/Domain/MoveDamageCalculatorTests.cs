using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.UnitTests.Domain;

public sealed class MoveDamageCalculatorTests
{
    [Fact]
    public void Calculate_Should_Use_Physical_Attack_And_Defense_For_Physical_Moves()
    {
        var result = MoveDamageCalculator.Calculate(new DamageCalculationInput(
            50,
            BaseStats.Create(90, 130, 80, 65, 85, 55),
            BaseStats.Create(100, 95, 80, 70, 85, 30),
            200,
            PokemonType.Fighting,
            MoveCategory.Physical,
            120,
            [PokemonType.Normal],
            100));

        Assert.Equal("Attack", result.OffensiveStat);
        Assert.Equal(130, result.OffensiveStatValue);
        Assert.Equal("Defense", result.DefensiveStat);
        Assert.Equal(80, result.DefensiveStatValue);
        Assert.Equal(171, result.RawDamage);
        Assert.Equal(171, result.Damage);
    }

    [Fact]
    public void Calculate_Should_Use_Special_Attack_And_Special_Defense_For_Special_Moves()
    {
        var result = MoveDamageCalculator.Calculate(new DamageCalculationInput(
            50,
            BaseStats.Create(90, 130, 80, 65, 85, 55),
            BaseStats.Create(100, 95, 80, 70, 85, 30),
            200,
            PokemonType.Water,
            MoveCategory.Special,
            120,
            [PokemonType.Normal],
            100));

        Assert.Equal("SpecialAttack", result.OffensiveStat);
        Assert.Equal(65, result.OffensiveStatValue);
        Assert.Equal("SpecialDefense", result.DefensiveStat);
        Assert.Equal(85, result.DefensiveStatValue);
        Assert.Equal(40, result.RawDamage);
        Assert.Equal(40, result.Damage);
    }

    [Fact]
    public void Calculate_Should_Clamp_Damage_To_Current_Health_Points_And_Never_Go_Below_Zero()
    {
        var result = MoveDamageCalculator.Calculate(new DamageCalculationInput(
            100,
            BaseStats.Create(90, 200, 80, 65, 85, 55),
            BaseStats.Create(100, 95, 10, 70, 85, 30),
            10,
            PokemonType.Fighting,
            MoveCategory.Physical,
            150,
            [PokemonType.Normal],
            100));

        Assert.True(result.RawDamage > 10);
        Assert.Equal(10, result.Damage);
        Assert.Equal(0, result.DefenderRemainingHealthPoints);
    }

    [Theory]
    [InlineData(PokemonType.Electric, PokemonType.Water, 2.0)]
    [InlineData(PokemonType.Grass, PokemonType.Fire, 0.5)]
    [InlineData(PokemonType.Grass, PokemonType.Flying, 0.5)]
    [InlineData(PokemonType.Electric, PokemonType.Ground, 0.0)]
    [InlineData(PokemonType.Dragon, PokemonType.Fairy, 0.0)]
    [InlineData(PokemonType.Fighting, PokemonType.Normal, 2.0)]
    public void Type_Chart_Should_Return_Normative_Coefficients(
        PokemonType attackingType,
        PokemonType defendingType,
        double expectedMultiplier)
    {
        var multiplier = PokemonTypeEffectivenessChart.GetMultiplier(attackingType, defendingType);

        Assert.Equal((decimal)expectedMultiplier, multiplier);
    }

    [Fact]
    public void Type_Chart_Should_Define_All_Type_Pairs_Using_Only_Normative_Coefficients()
    {
        var allowedMultipliers = new HashSet<decimal> { 0m, 0.5m, 1m, 2m };

        foreach (var attackingType in Enum.GetValues<PokemonType>())
        {
            foreach (var defendingType in Enum.GetValues<PokemonType>())
            {
                var multiplier = PokemonTypeEffectivenessChart.GetMultiplier(attackingType, defendingType);
                Assert.Contains(multiplier, allowedMultipliers);
            }
        }
    }
}
