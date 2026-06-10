using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Visiotech.Pokemon.Api.Contracts;
using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.Pokemons.Commands.CreatePokemonSpecies;
using Visiotech.Pokemon.Application.Features.Pokemons.Commands.DeletePokemonSpecies;
using Visiotech.Pokemon.Application.Features.Pokemons.Commands.UpdatePokemonSpeciesLearnableMoves;
using Visiotech.Pokemon.Application.Features.Pokemons.Commands.UpdatePokemonSpecies;
using Visiotech.Pokemon.Application.Features.Pokemons.Queries.GetPokemonSpeciesLearnableMoves;
using Visiotech.Pokemon.Application.Features.Pokemons.Queries.GetPokemonSpeciesDetail;
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

        endpoints.MapPut("/api/v1/pokemons/{id:guid}", UpdatePokemonSpeciesAsync)
            .WithName("UpdatePokemonSpecies")
            .WithSummary("Updates a base pokemon species in the catalog.")
            .Produces<PokemonSpeciesContract>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        endpoints.MapDelete("/api/v1/pokemons/{id:guid}", DeletePokemonSpeciesAsync)
            .WithName("DeletePokemonSpecies")
            .WithSummary("Deletes a base pokemon species from the catalog when it has no active dependencies.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        endpoints.MapPut("/api/v1/pokemons/{id:guid}/learnable-moves", UpdatePokemonSpeciesLearnableMovesAsync)
            .WithName("UpdatePokemonSpeciesLearnableMoves")
            .WithSummary("Associates or removes learnable moves for a base pokemon species.")
            .Produces<PokemonLearnableMovesContract>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        endpoints.MapGet("/api/v1/pokemons", GetPokemonsCatalogAsync)
            .WithName("GetPokemonsCatalog")
            .WithSummary("Returns the base pokemon species catalog exposed by the API.")
            .Produces<PokemonSpeciesCatalogContract>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        endpoints.MapGet("/api/v1/pokemons/{id:guid}", GetPokemonSpeciesDetailAsync)
            .WithName("GetPokemonSpeciesDetail")
            .WithSummary("Returns the detail of a base pokemon species.")
            .Produces<PokemonSpeciesContract>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        endpoints.MapGet("/api/v1/pokemons/{id:guid}/learnable-moves", GetPokemonSpeciesLearnableMovesAsync)
            .WithName("GetPokemonSpeciesLearnableMoves")
            .WithSummary("Returns the learnable move catalog for a base pokemon species.")
            .Produces<PokemonLearnableMovesContract>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

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

    private static async Task<Ok<PokemonSpeciesContract>> UpdatePokemonSpeciesAsync(
        Guid id,
        UpdatePokemonSpeciesRequestContract request,
        ICommandHandler<UpdatePokemonSpeciesCommand, PokemonSpeciesResponse> handler,
        CancellationToken cancellationToken)
    {
        var baseStats = request.BaseStats;
        var response = await handler.Handle(
            new UpdatePokemonSpeciesCommand(
                id,
                request.Name,
                request.Types,
                baseStats?.Health ?? 0,
                baseStats?.Attack ?? 0,
                baseStats?.Defense ?? 0,
                baseStats?.SpecialAttack ?? 0,
                baseStats?.SpecialDefense ?? 0,
                baseStats?.Speed ?? 0),
            cancellationToken);

        return TypedResults.Ok(response.ToContract());
    }

    private static async Task<NoContent> DeletePokemonSpeciesAsync(
        Guid id,
        ICommandHandler<DeletePokemonSpeciesCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new DeletePokemonSpeciesCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<PokemonLearnableMovesContract>> UpdatePokemonSpeciesLearnableMovesAsync(
        Guid id,
        UpdatePokemonLearnableMovesRequestContract request,
        ICommandHandler<UpdatePokemonSpeciesLearnableMovesCommand, PokemonLearnableMovesResponse> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(
            new UpdatePokemonSpeciesLearnableMovesCommand(
                id,
                request.AddMoveIds,
                request.RemoveMoveIds),
            cancellationToken);

        return TypedResults.Ok(response.ToContract());
    }

    private static async Task<Ok<PokemonSpeciesCatalogContract>> GetPokemonsCatalogAsync(
        string? name,
        string? type,
        int? page,
        int? pageSize,
        IQueryHandler<GetPokemonsCatalogQuery, PokemonSpeciesCatalogResponse> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(
            new GetPokemonsCatalogQuery(name, type, page ?? 1, pageSize ?? 20),
            cancellationToken);

        return TypedResults.Ok(response.ToContract());
    }

    private static async Task<Ok<PokemonSpeciesContract>> GetPokemonSpeciesDetailAsync(
        Guid id,
        IQueryHandler<GetPokemonSpeciesDetailQuery, PokemonSpeciesResponse> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(new GetPokemonSpeciesDetailQuery(id), cancellationToken);
        return TypedResults.Ok(response.ToContract());
    }

    private static async Task<Ok<PokemonLearnableMovesContract>> GetPokemonSpeciesLearnableMovesAsync(
        Guid id,
        IQueryHandler<GetPokemonSpeciesLearnableMovesQuery, PokemonLearnableMovesResponse> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(new GetPokemonSpeciesLearnableMovesQuery(id), cancellationToken);
        return TypedResults.Ok(response.ToContract());
    }
}
