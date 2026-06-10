using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Visiotech.Pokemon.Api.Contracts;
using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.Pokemons.Commands.CreatePokemonSpecies;
using Visiotech.Pokemon.Application.Features.Pokemons.Queries.GetPokemonsCatalog;
using Visiotech.Pokemon.Contracts;

namespace Visiotech.Pokemon.Api;

public static class PokemonEndpoints
{
    public static IEndpointRouteBuilder MapPokemonEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/v1/pokemons", CreatePokemonSpeciesAsync)
            .WithName("CreatePokemonSpecies")
            .WithSummary("Creates a base pokemon species in the catalog.")
            .Produces<PokemonSpeciesContract>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        endpoints.MapGet("/api/v1/pokemons", GetPokemonsCatalogAsync)
            .WithName("GetPokemonsCatalog")
            .WithSummary("Returns the base pokemon species catalog exposed by the API.")
            .Produces<IReadOnlyCollection<PokemonSpeciesContract>>(StatusCodes.Status200OK);

        return endpoints;
    }

    private static async Task<Created<PokemonSpeciesContract>> CreatePokemonSpeciesAsync(
        CreatePokemonSpeciesRequestContract request,
        ICommandHandler<CreatePokemonSpeciesCommand, PokemonSpeciesResponse> handler,
        CancellationToken cancellationToken)
    {
        var baseStats = request.BaseStats;
        var response = await handler.Handle(
            new CreatePokemonSpeciesCommand(
                request.Name,
                request.Types,
                baseStats?.Health ?? 0,
                baseStats?.Attack ?? 0,
                baseStats?.Defense ?? 0,
                baseStats?.SpecialAttack ?? 0,
                baseStats?.SpecialDefense ?? 0,
                baseStats?.Speed ?? 0),
            cancellationToken);

        return TypedResults.Created($"/api/v1/pokemons/{response.Id}", response.ToContract());
    }

    private static async Task<Ok<PokemonSpeciesContract[]>> GetPokemonsCatalogAsync(
        IQueryHandler<GetPokemonsCatalogQuery, IReadOnlyCollection<PokemonSpeciesResponse>> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(new GetPokemonsCatalogQuery(), cancellationToken);
        return TypedResults.Ok(response.Select(item => item.ToContract()).ToArray());
    }
}
