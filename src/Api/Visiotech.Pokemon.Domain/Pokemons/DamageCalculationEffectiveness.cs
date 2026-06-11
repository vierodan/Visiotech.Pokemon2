namespace Visiotech.Pokemon.Domain.Pokemons;

public sealed record DamageCalculationEffectiveness(
    PokemonType DefenderType,
    decimal Multiplier);
