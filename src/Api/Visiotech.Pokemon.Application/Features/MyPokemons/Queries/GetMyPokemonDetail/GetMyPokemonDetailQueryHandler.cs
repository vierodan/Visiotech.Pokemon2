using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Application.Features.MyPokemons.Queries.GetMyPokemonDetail;

public sealed class GetMyPokemonDetailQueryHandler(
    IMyPokemonReadRepository repository,
    IPokemonSpeciesReadRepository speciesRepository,
    IPokemonMoveReadRepository moveRepository)
    : IQueryHandler<GetMyPokemonDetailQuery, MyPokemonResponse>
{
    public async Task<MyPokemonResponse> Handle(
        GetMyPokemonDetailQuery query,
        CancellationToken cancellationToken)
    {
        var myPokemon = await repository.GetByIdAsync(query.Id, cancellationToken);
        if (myPokemon is null)
        {
            throw new ApplicationNotFoundException(
                $"My pokemon '{query.Id}' was not found.",
                "id");
        }

        var pokemonSpecies = await speciesRepository.GetByIdAsync(myPokemon.PokemonSpeciesId, cancellationToken)
            ?? throw new InvalidOperationException($"Pokemon species '{myPokemon.PokemonSpeciesId}' referenced by my pokemon '{myPokemon.Id}' was not found.");

        var equippedMoves = await moveRepository.GetByIdsAsync(myPokemon.EquippedMoveIds, cancellationToken);
        return MyPokemonMapping.ToResponse(myPokemon, pokemonSpecies, equippedMoves);
    }
}
