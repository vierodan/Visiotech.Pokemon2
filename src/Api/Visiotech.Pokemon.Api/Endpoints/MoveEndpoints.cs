using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Visiotech.Pokemon.Api.Contracts;
using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.Moves.Commands.CreatePokemonMove;
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
}
