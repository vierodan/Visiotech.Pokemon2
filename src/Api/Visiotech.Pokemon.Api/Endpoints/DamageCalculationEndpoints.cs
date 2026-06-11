using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Visiotech.Pokemon.Api.Contracts;
using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.Damage.Queries.CalculateMoveDamage;
using Visiotech.Pokemon.Contracts;

namespace Visiotech.Pokemon.Api;

public static class DamageCalculationEndpoints
{
    public static IEndpointRouteBuilder MapDamageCalculationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/v1/damage-calculations", CalculateMoveDamageAsync)
            .WithName("CalculateMoveDamage")
            .WithSummary("Calculates the damage a move would deal from one playable pokemon instance to another.")
            .Produces<MoveDamageCalculationContract>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static async Task<Ok<MoveDamageCalculationContract>> CalculateMoveDamageAsync(
        CalculateMoveDamageRequestContract request,
        IQueryHandler<CalculateMoveDamageQuery, MoveDamageCalculationResponse> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(
            new CalculateMoveDamageQuery(
                request.AttackerMyPokemonId,
                request.DefenderMyPokemonId,
                request.MoveId),
            cancellationToken);

        return TypedResults.Ok(response.ToContract());
    }
}
