using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.Moves.Queries;
using Visiotech.Pokemon.Application.Features.Pokemons.Queries;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Application.Features.MyPokemons.Queries;

internal static class MyPokemonMapping
{
    public static MyPokemonResponse ToResponse(
        MyPokemon myPokemon,
        PokemonSpecies species,
        IReadOnlyCollection<PokemonMove> equippedMoves)
    {
        var movesById = equippedMoves.ToDictionary(move => move.Id);
        var orderedMoves = myPokemon.EquippedMoveIds
            .Where(movesById.ContainsKey)
            .Select(moveId => movesById[moveId])
            .Select(PokemonMoveMapping.ToResponse)
            .ToArray();

        return new MyPokemonResponse(
            myPokemon.Id,
            PokemonSpeciesMapping.ToResponse(species),
            myPokemon.Level.Value,
            myPokemon.CurrentHealthPoints,
            myPokemon.TotalHealthPoints,
            orderedMoves);
    }
}
