using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Application.Features.Pokemons.Queries;

internal static class PokemonSpeciesMapping
{
    public static PokemonSpeciesResponse ToResponse(PokemonSpecies pokemonSpecies) =>
        new(
            pokemonSpecies.Id,
            pokemonSpecies.Name.Value,
            pokemonSpecies.Types.Select(static type => type.ToString()).ToArray(),
            new PokemonBaseStatsResponse(
                pokemonSpecies.BaseStats.Health,
                pokemonSpecies.BaseStats.Attack,
                pokemonSpecies.BaseStats.Defense,
                pokemonSpecies.BaseStats.SpecialAttack,
                pokemonSpecies.BaseStats.SpecialDefense,
                pokemonSpecies.BaseStats.Speed));
}
