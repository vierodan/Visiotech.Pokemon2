using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Visiotech.Pokemon.Api.Contracts;
using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.MyPokemons.Commands.CreateMyPokemon;
using Visiotech.Pokemon.Application.Features.MyPokemons.Commands.DeleteMyPokemon;
using Visiotech.Pokemon.Application.Features.MyPokemons.Queries.GetMyPokemonEquippedMoves;
using Visiotech.Pokemon.Application.Features.MyPokemons.Commands.UpdateMyPokemon;
using Visiotech.Pokemon.Application.Features.MyPokemons.Queries.GetMyPokemonDetail;
using Visiotech.Pokemon.Application.Features.MyPokemons.Queries.GetMyPokemonsCatalog;
using Visiotech.Pokemon.Contracts;

namespace Visiotech.Pokemon.Api;

public static class MyPokemonEndpoints
{
    public static IEndpointRouteBuilder MapMyPokemonEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/v1/my-pokemons", CreateMyPokemonAsync)
            .WithName("CreateMyPokemon")
            .WithSummary("Creates a playable pokemon instance based on a base species.")
            .Produces<MyPokemonContract>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        endpoints.MapPut("/api/v1/my-pokemons/{id:guid}", UpdateMyPokemonAsync)
            .WithName("UpdateMyPokemon")
            .WithSummary("Updates the mutable state of a playable pokemon instance.")
            .Produces<MyPokemonContract>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        endpoints.MapDelete("/api/v1/my-pokemons/{id:guid}", DeleteMyPokemonAsync)
            .WithName("DeleteMyPokemon")
            .WithSummary("Deletes a playable pokemon instance when it is not used by active dependencies.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        endpoints.MapGet("/api/v1/my-pokemons", GetMyPokemonsCatalogAsync)
            .WithName("GetMyPokemonsCatalog")
            .WithSummary("Returns the playable pokemon instances registered in the API.")
            .Produces<MyPokemonCatalogContract>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        endpoints.MapGet("/api/v1/my-pokemons/{id:guid}", GetMyPokemonDetailAsync)
            .WithName("GetMyPokemonDetail")
            .WithSummary("Returns the detail of a playable pokemon instance.")
            .Produces<MyPokemonContract>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        endpoints.MapGet("/api/v1/my-pokemons/{id:guid}/equipped-moves", GetMyPokemonEquippedMovesAsync)
            .WithName("GetMyPokemonEquippedMoves")
            .WithSummary("Returns the moves currently equipped by a playable pokemon instance.")
            .Produces<MyPokemonEquippedMovesContract>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static async Task<Created<MyPokemonContract>> CreateMyPokemonAsync(
        CreateMyPokemonRequestContract request,
        ICommandHandler<CreateMyPokemonCommand, MyPokemonResponse> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(
            new CreateMyPokemonCommand(
                request.PokemonSpeciesId,
                request.Level,
                request.CurrentHealthPoints,
                request.TotalHealthPoints,
                request.EquippedMoveIds),
            cancellationToken);

        return TypedResults.Created($"/api/v1/my-pokemons/{response.Id}", response.ToContract());
    }

    private static async Task<Ok<MyPokemonContract>> UpdateMyPokemonAsync(
        Guid id,
        UpdateMyPokemonRequestContract request,
        ICommandHandler<UpdateMyPokemonCommand, MyPokemonResponse> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(
            new UpdateMyPokemonCommand(
                id,
                request.Level,
                request.CurrentHealthPoints,
                request.TotalHealthPoints,
                request.EquippedMoveIds),
            cancellationToken);

        return TypedResults.Ok(response.ToContract());
    }

    private static async Task<NoContent> DeleteMyPokemonAsync(
        Guid id,
        ICommandHandler<DeleteMyPokemonCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new DeleteMyPokemonCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<MyPokemonCatalogContract>> GetMyPokemonsCatalogAsync(
        int? page,
        int? pageSize,
        IQueryHandler<GetMyPokemonsCatalogQuery, MyPokemonCatalogResponse> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(
            new GetMyPokemonsCatalogQuery(page ?? 1, pageSize ?? 20),
            cancellationToken);

        return TypedResults.Ok(response.ToContract());
    }

    private static async Task<Ok<MyPokemonContract>> GetMyPokemonDetailAsync(
        Guid id,
        IQueryHandler<GetMyPokemonDetailQuery, MyPokemonResponse> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(new GetMyPokemonDetailQuery(id), cancellationToken);
        return TypedResults.Ok(response.ToContract());
    }

    private static async Task<Ok<MyPokemonEquippedMovesContract>> GetMyPokemonEquippedMovesAsync(
        Guid id,
        IQueryHandler<GetMyPokemonEquippedMovesQuery, MyPokemonEquippedMovesResponse> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(new GetMyPokemonEquippedMovesQuery(id), cancellationToken);
        return TypedResults.Ok(response.ToContract());
    }
}
