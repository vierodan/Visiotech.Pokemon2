using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Visiotech.Pokemon.Api.Contracts;
using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.MyPokemons.Commands.CreateMyPokemon;
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
}
