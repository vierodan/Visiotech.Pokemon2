using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Visiotech.Pokemon.Api.Contracts;
using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.Moves.Commands.CreatePokemonMove;
using Visiotech.Pokemon.Application.Features.Moves.Queries.GetPokemonMoveDetail;
using Visiotech.Pokemon.Application.Features.Moves.Queries.GetPokemonMovesCatalog;
using Visiotech.Pokemon.Contracts;

namespace Visiotech.Pokemon.Api;

public static class MoveEndpoints
{
    public static IEndpointRouteBuilder MapMoveEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/v1/moves", CreatePokemonMoveAsync)
            .WithName("CreatePokemonMove")
            .WithSummary("Creates a move in the catalog.")
            .Produces<PokemonMoveContract>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        endpoints.MapGet("/api/v1/moves", GetPokemonMovesCatalogAsync)
            .WithName("GetPokemonMovesCatalog")
            .WithSummary("Returns the move catalog exposed by the API.")
            .Produces<PokemonMoveCatalogContract>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        endpoints.MapGet("/api/v1/moves/{id:guid}", GetPokemonMoveDetailAsync)
            .WithName("GetPokemonMoveDetail")
            .WithSummary("Returns the detail of a move.")
            .Produces<PokemonMoveContract>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static async Task<Created<PokemonMoveContract>> CreatePokemonMoveAsync(
        CreatePokemonMoveRequestContract request,
        ICommandHandler<CreatePokemonMoveCommand, PokemonMoveResponse> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(
            new CreatePokemonMoveCommand(
                request.Name,
                request.Type,
                request.Category,
                request.Power),
            cancellationToken);

        return TypedResults.Created($"/api/v1/moves/{response.Id}", response.ToContract());
    }

    private static async Task<Ok<PokemonMoveCatalogContract>> GetPokemonMovesCatalogAsync(
        string? name,
        string? type,
        string? category,
        int? page,
        int? pageSize,
        IQueryHandler<GetPokemonMovesCatalogQuery, PokemonMoveCatalogResponse> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(
            new GetPokemonMovesCatalogQuery(name, type, category, page ?? 1, pageSize ?? 20),
            cancellationToken);

        return TypedResults.Ok(response.ToContract());
    }

    private static async Task<Ok<PokemonMoveContract>> GetPokemonMoveDetailAsync(
        Guid id,
        IQueryHandler<GetPokemonMoveDetailQuery, PokemonMoveResponse> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(new GetPokemonMoveDetailQuery(id), cancellationToken);
        return TypedResults.Ok(response.ToContract());
    }
}
