using Visiotech.Pokemon.Domain.Abstractions;

namespace Visiotech.Pokemon.Domain.Pokemons;

public static class PokemonTypeEffectivenessChart
{
    private static readonly IReadOnlyDictionary<PokemonType, IReadOnlyDictionary<PokemonType, decimal>> Chart =
        new Dictionary<PokemonType, IReadOnlyDictionary<PokemonType, decimal>>
        {
            [PokemonType.Steel] = CreateRow(
                (PokemonType.Steel, 0.5m), (PokemonType.Water, 0.5m), (PokemonType.Bug, 1m), (PokemonType.Dragon, 1m),
                (PokemonType.Electric, 0.5m), (PokemonType.Ghost, 1m), (PokemonType.Fire, 0.5m), (PokemonType.Fairy, 2m),
                (PokemonType.Ice, 2m), (PokemonType.Fighting, 1m), (PokemonType.Normal, 1m), (PokemonType.Grass, 1m),
                (PokemonType.Psychic, 1m), (PokemonType.Rock, 2m), (PokemonType.Dark, 1m), (PokemonType.Ground, 1m),
                (PokemonType.Poison, 1m), (PokemonType.Flying, 1m)),
            [PokemonType.Water] = CreateRow(
                (PokemonType.Steel, 1m), (PokemonType.Water, 0.5m), (PokemonType.Bug, 1m), (PokemonType.Dragon, 0.5m),
                (PokemonType.Electric, 1m), (PokemonType.Ghost, 1m), (PokemonType.Fire, 2m), (PokemonType.Fairy, 1m),
                (PokemonType.Ice, 1m), (PokemonType.Fighting, 1m), (PokemonType.Normal, 1m), (PokemonType.Grass, 0.5m),
                (PokemonType.Psychic, 1m), (PokemonType.Rock, 2m), (PokemonType.Dark, 1m), (PokemonType.Ground, 2m),
                (PokemonType.Poison, 1m), (PokemonType.Flying, 1m)),
            [PokemonType.Bug] = CreateRow(
                (PokemonType.Steel, 0.5m), (PokemonType.Water, 1m), (PokemonType.Bug, 1m), (PokemonType.Dragon, 1m),
                (PokemonType.Electric, 1m), (PokemonType.Ghost, 0.5m), (PokemonType.Fire, 0.5m), (PokemonType.Fairy, 0.5m),
                (PokemonType.Ice, 1m), (PokemonType.Fighting, 0.5m), (PokemonType.Normal, 1m), (PokemonType.Grass, 2m),
                (PokemonType.Psychic, 2m), (PokemonType.Rock, 1m), (PokemonType.Dark, 2m), (PokemonType.Ground, 1m),
                (PokemonType.Poison, 0.5m), (PokemonType.Flying, 0.5m)),
            [PokemonType.Dragon] = CreateRow(
                (PokemonType.Steel, 0.5m), (PokemonType.Water, 1m), (PokemonType.Bug, 1m), (PokemonType.Dragon, 2m),
                (PokemonType.Electric, 1m), (PokemonType.Ghost, 1m), (PokemonType.Fire, 1m), (PokemonType.Fairy, 0m),
                (PokemonType.Ice, 1m), (PokemonType.Fighting, 1m), (PokemonType.Normal, 1m), (PokemonType.Grass, 1m),
                (PokemonType.Psychic, 1m), (PokemonType.Rock, 1m), (PokemonType.Dark, 1m), (PokemonType.Ground, 1m),
                (PokemonType.Poison, 1m), (PokemonType.Flying, 1m)),
            [PokemonType.Electric] = CreateRow(
                (PokemonType.Steel, 1m), (PokemonType.Water, 2m), (PokemonType.Bug, 1m), (PokemonType.Dragon, 0.5m),
                (PokemonType.Electric, 0.5m), (PokemonType.Ghost, 1m), (PokemonType.Fire, 1m), (PokemonType.Fairy, 1m),
                (PokemonType.Ice, 1m), (PokemonType.Fighting, 1m), (PokemonType.Normal, 1m), (PokemonType.Grass, 0.5m),
                (PokemonType.Psychic, 1m), (PokemonType.Rock, 1m), (PokemonType.Dark, 1m), (PokemonType.Ground, 0m),
                (PokemonType.Poison, 1m), (PokemonType.Flying, 2m)),
            [PokemonType.Ghost] = CreateRow(
                (PokemonType.Steel, 1m), (PokemonType.Water, 1m), (PokemonType.Bug, 1m), (PokemonType.Dragon, 1m),
                (PokemonType.Electric, 1m), (PokemonType.Ghost, 2m), (PokemonType.Fire, 1m), (PokemonType.Fairy, 1m),
                (PokemonType.Ice, 1m), (PokemonType.Fighting, 1m), (PokemonType.Normal, 0m), (PokemonType.Grass, 1m),
                (PokemonType.Psychic, 2m), (PokemonType.Rock, 1m), (PokemonType.Dark, 0.5m), (PokemonType.Ground, 1m),
                (PokemonType.Poison, 1m), (PokemonType.Flying, 1m)),
            [PokemonType.Fire] = CreateRow(
                (PokemonType.Steel, 2m), (PokemonType.Water, 0.5m), (PokemonType.Bug, 2m), (PokemonType.Dragon, 0.5m),
                (PokemonType.Electric, 1m), (PokemonType.Ghost, 1m), (PokemonType.Fire, 0.5m), (PokemonType.Fairy, 1m),
                (PokemonType.Ice, 2m), (PokemonType.Fighting, 1m), (PokemonType.Normal, 1m), (PokemonType.Grass, 2m),
                (PokemonType.Psychic, 1m), (PokemonType.Rock, 0.5m), (PokemonType.Dark, 1m), (PokemonType.Ground, 1m),
                (PokemonType.Poison, 1m), (PokemonType.Flying, 1m)),
            [PokemonType.Fairy] = CreateRow(
                (PokemonType.Steel, 0.5m), (PokemonType.Water, 1m), (PokemonType.Bug, 1m), (PokemonType.Dragon, 2m),
                (PokemonType.Electric, 1m), (PokemonType.Ghost, 1m), (PokemonType.Fire, 0.5m), (PokemonType.Fairy, 1m),
                (PokemonType.Ice, 1m), (PokemonType.Fighting, 2m), (PokemonType.Normal, 1m), (PokemonType.Grass, 1m),
                (PokemonType.Psychic, 1m), (PokemonType.Rock, 1m), (PokemonType.Dark, 2m), (PokemonType.Ground, 1m),
                (PokemonType.Poison, 0.5m), (PokemonType.Flying, 1m)),
            [PokemonType.Ice] = CreateRow(
                (PokemonType.Steel, 0.5m), (PokemonType.Water, 0.5m), (PokemonType.Bug, 1m), (PokemonType.Dragon, 2m),
                (PokemonType.Electric, 1m), (PokemonType.Ghost, 1m), (PokemonType.Fire, 0.5m), (PokemonType.Fairy, 1m),
                (PokemonType.Ice, 0.5m), (PokemonType.Fighting, 1m), (PokemonType.Normal, 1m), (PokemonType.Grass, 2m),
                (PokemonType.Psychic, 1m), (PokemonType.Rock, 1m), (PokemonType.Dark, 1m), (PokemonType.Ground, 2m),
                (PokemonType.Poison, 1m), (PokemonType.Flying, 2m)),
            [PokemonType.Fighting] = CreateRow(
                (PokemonType.Steel, 2m), (PokemonType.Water, 1m), (PokemonType.Bug, 0.5m), (PokemonType.Dragon, 1m),
                (PokemonType.Electric, 1m), (PokemonType.Ghost, 0m), (PokemonType.Fire, 1m), (PokemonType.Fairy, 0.5m),
                (PokemonType.Ice, 2m), (PokemonType.Fighting, 1m), (PokemonType.Normal, 2m), (PokemonType.Grass, 1m),
                (PokemonType.Psychic, 0.5m), (PokemonType.Rock, 2m), (PokemonType.Dark, 2m), (PokemonType.Ground, 1m),
                (PokemonType.Poison, 0.5m), (PokemonType.Flying, 0.5m)),
            [PokemonType.Normal] = CreateRow(
                (PokemonType.Steel, 0.5m), (PokemonType.Water, 1m), (PokemonType.Bug, 1m), (PokemonType.Dragon, 1m),
                (PokemonType.Electric, 1m), (PokemonType.Ghost, 0m), (PokemonType.Fire, 1m), (PokemonType.Fairy, 1m),
                (PokemonType.Ice, 1m), (PokemonType.Fighting, 1m), (PokemonType.Normal, 1m), (PokemonType.Grass, 1m),
                (PokemonType.Psychic, 1m), (PokemonType.Rock, 0.5m), (PokemonType.Dark, 1m), (PokemonType.Ground, 1m),
                (PokemonType.Poison, 1m), (PokemonType.Flying, 1m)),
            [PokemonType.Grass] = CreateRow(
                (PokemonType.Steel, 0.5m), (PokemonType.Water, 2m), (PokemonType.Bug, 0.5m), (PokemonType.Dragon, 0.5m),
                (PokemonType.Electric, 1m), (PokemonType.Ghost, 1m), (PokemonType.Fire, 0.5m), (PokemonType.Fairy, 1m),
                (PokemonType.Ice, 1m), (PokemonType.Fighting, 1m), (PokemonType.Normal, 1m), (PokemonType.Grass, 0.5m),
                (PokemonType.Psychic, 1m), (PokemonType.Rock, 2m), (PokemonType.Dark, 1m), (PokemonType.Ground, 2m),
                (PokemonType.Poison, 0.5m), (PokemonType.Flying, 0.5m)),
            [PokemonType.Psychic] = CreateRow(
                (PokemonType.Steel, 0.5m), (PokemonType.Water, 1m), (PokemonType.Bug, 1m), (PokemonType.Dragon, 1m),
                (PokemonType.Electric, 1m), (PokemonType.Ghost, 1m), (PokemonType.Fire, 1m), (PokemonType.Fairy, 1m),
                (PokemonType.Ice, 1m), (PokemonType.Fighting, 2m), (PokemonType.Normal, 1m), (PokemonType.Grass, 1m),
                (PokemonType.Psychic, 0.5m), (PokemonType.Rock, 1m), (PokemonType.Dark, 0m), (PokemonType.Ground, 1m),
                (PokemonType.Poison, 2m), (PokemonType.Flying, 1m)),
            [PokemonType.Rock] = CreateRow(
                (PokemonType.Steel, 0.5m), (PokemonType.Water, 1m), (PokemonType.Bug, 2m), (PokemonType.Dragon, 1m),
                (PokemonType.Electric, 1m), (PokemonType.Ghost, 1m), (PokemonType.Fire, 2m), (PokemonType.Fairy, 1m),
                (PokemonType.Ice, 2m), (PokemonType.Fighting, 0.5m), (PokemonType.Normal, 1m), (PokemonType.Grass, 1m),
                (PokemonType.Psychic, 1m), (PokemonType.Rock, 1m), (PokemonType.Dark, 1m), (PokemonType.Ground, 0.5m),
                (PokemonType.Poison, 1m), (PokemonType.Flying, 2m)),
            [PokemonType.Dark] = CreateRow(
                (PokemonType.Steel, 1m), (PokemonType.Water, 1m), (PokemonType.Bug, 1m), (PokemonType.Dragon, 1m),
                (PokemonType.Electric, 1m), (PokemonType.Ghost, 2m), (PokemonType.Fire, 1m), (PokemonType.Fairy, 0.5m),
                (PokemonType.Ice, 1m), (PokemonType.Fighting, 0.5m), (PokemonType.Normal, 1m), (PokemonType.Grass, 1m),
                (PokemonType.Psychic, 2m), (PokemonType.Rock, 1m), (PokemonType.Dark, 0.5m), (PokemonType.Ground, 1m),
                (PokemonType.Poison, 1m), (PokemonType.Flying, 1m)),
            [PokemonType.Ground] = CreateRow(
                (PokemonType.Steel, 2m), (PokemonType.Water, 1m), (PokemonType.Bug, 0.5m), (PokemonType.Dragon, 1m),
                (PokemonType.Electric, 2m), (PokemonType.Ghost, 1m), (PokemonType.Fire, 2m), (PokemonType.Fairy, 1m),
                (PokemonType.Ice, 1m), (PokemonType.Fighting, 1m), (PokemonType.Normal, 1m), (PokemonType.Grass, 0.5m),
                (PokemonType.Psychic, 1m), (PokemonType.Rock, 2m), (PokemonType.Dark, 1m), (PokemonType.Ground, 1m),
                (PokemonType.Poison, 2m), (PokemonType.Flying, 0m)),
            [PokemonType.Poison] = CreateRow(
                (PokemonType.Steel, 0m), (PokemonType.Water, 1m), (PokemonType.Bug, 1m), (PokemonType.Dragon, 1m),
                (PokemonType.Electric, 1m), (PokemonType.Ghost, 0.5m), (PokemonType.Fire, 1m), (PokemonType.Fairy, 2m),
                (PokemonType.Ice, 1m), (PokemonType.Fighting, 1m), (PokemonType.Normal, 1m), (PokemonType.Grass, 2m),
                (PokemonType.Psychic, 1m), (PokemonType.Rock, 0.5m), (PokemonType.Dark, 1m), (PokemonType.Ground, 0.5m),
                (PokemonType.Poison, 0.5m), (PokemonType.Flying, 1m)),
            [PokemonType.Flying] = CreateRow(
                (PokemonType.Steel, 0.5m), (PokemonType.Water, 1m), (PokemonType.Bug, 2m), (PokemonType.Dragon, 1m),
                (PokemonType.Electric, 0.5m), (PokemonType.Ghost, 1m), (PokemonType.Fire, 1m), (PokemonType.Fairy, 1m),
                (PokemonType.Ice, 1m), (PokemonType.Fighting, 2m), (PokemonType.Normal, 1m), (PokemonType.Grass, 2m),
                (PokemonType.Psychic, 1m), (PokemonType.Rock, 0.5m), (PokemonType.Dark, 1m), (PokemonType.Ground, 1m),
                (PokemonType.Poison, 1m), (PokemonType.Flying, 1m))
        };

    public static decimal GetMultiplier(PokemonType attackingType, PokemonType defendingType)
    {
        if (!Chart.TryGetValue(attackingType, out var row) || !row.TryGetValue(defendingType, out var multiplier))
        {
            throw new DomainException($"Type effectiveness is not defined for attacker '{attackingType}' and defender '{defendingType}'.");
        }

        return multiplier;
    }

    private static IReadOnlyDictionary<PokemonType, decimal> CreateRow(params (PokemonType Type, decimal Multiplier)[] values) =>
        values.ToDictionary(static item => item.Type, static item => item.Multiplier);
}
