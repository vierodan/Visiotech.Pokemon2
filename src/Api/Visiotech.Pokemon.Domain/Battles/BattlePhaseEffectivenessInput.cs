using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Domain.Battles;

public sealed record BattlePhaseEffectivenessInput(
    PokemonType DefenderType,
    decimal Multiplier);
