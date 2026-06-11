using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Visiotech.Pokemon.Api.Contracts;
using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.Battles.Commands.CreateBattle;
using Visiotech.Pokemon.Contracts;

namespace Visiotech.Pokemon.Api;

public static class BattleEndpoints
{
    public static IEndpointRouteBuilder MapBattleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/v1/battles", CreateBattleAsync)
            .WithName("CreateBattle")
            .WithSummary("Creates a battle between exactly two playable pokemon instances.")
            .Produces<BattleContract>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static async Task<Created<BattleContract>> CreateBattleAsync(
        CreateBattleRequestContract request,
        ICommandHandler<CreateBattleCommand, BattleResponse> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(
            new CreateBattleCommand(
                request.FirstMyPokemonId,
                request.SecondMyPokemonId),
            cancellationToken);

        return TypedResults.Created($"/api/v1/battles/{response.Id}", response.ToContract());
    }
}
