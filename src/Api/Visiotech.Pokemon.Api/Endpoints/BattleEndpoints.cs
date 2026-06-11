using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Visiotech.Pokemon.Api.Contracts;
using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.Battles.Commands.CreateBattle;
using Visiotech.Pokemon.Application.Features.Battles.Commands.ExecuteBattlePhase;
using Visiotech.Pokemon.Application.Features.Battles.Queries.GetBattleHistory;
using Visiotech.Pokemon.Application.Features.Battles.Queries.GetBattleState;
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

        endpoints.MapGet("/api/v1/battles/{id:guid}", GetBattleStateAsync)
            .WithName("GetBattleState")
            .WithSummary("Returns the current state and recorded history of a battle.")
            .Produces<BattleContract>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        endpoints.MapPost("/api/v1/battles/{id:guid}/phases", ExecuteBattlePhaseAsync)
            .WithName("ExecuteBattlePhase")
            .WithSummary("Executes the next battle phase for a battle using an equipped move.")
            .Produces<BattlePhaseExecutionContract>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        endpoints.MapGet("/api/v1/battles/{id:guid}/phases", GetBattleHistoryAsync)
            .WithName("GetBattleHistory")
            .WithSummary("Returns the ordered history of recorded battle phases.")
            .Produces<BattleHistoryContract>(StatusCodes.Status200OK)
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

    private static async Task<Ok<BattleContract>> GetBattleStateAsync(
        Guid id,
        IQueryHandler<GetBattleStateQuery, BattleResponse> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(new GetBattleStateQuery(id), cancellationToken);
        return TypedResults.Ok(response.ToContract());
    }

    private static async Task<Ok<BattlePhaseExecutionContract>> ExecuteBattlePhaseAsync(
        Guid id,
        ExecuteBattlePhaseRequestContract request,
        ICommandHandler<ExecuteBattlePhaseCommand, BattlePhaseExecutionResponse> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(
            new ExecuteBattlePhaseCommand(
                id,
                request.AttackerMyPokemonId,
                request.MoveId),
            cancellationToken);

        return TypedResults.Ok(response.ToContract());
    }

    private static async Task<Ok<BattleHistoryContract>> GetBattleHistoryAsync(
        Guid id,
        IQueryHandler<GetBattleHistoryQuery, BattleHistoryResponse> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(new GetBattleHistoryQuery(id), cancellationToken);
        return TypedResults.Ok(response.ToContract());
    }
}
