using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Visiotech.Pokemon.Api.Contracts;
using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.Pokemons.Queries.GetPokemonsCatalog;
using Visiotech.Pokemon.Contracts;

namespace Visiotech.Pokemon.Api;

public static class PokemonEndpoints
{
    public static IEndpointRouteBuilder MapPokemonEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/pokemons", GetPokemonsCatalogAsync)
            .WithName("GetPokemonsCatalog")
            .WithSummary("Returns the pokemon catalog exposed by the API.")
            .Produces<IReadOnlyCollection<PokemonContract>>(StatusCodes.Status200OK);

        return endpoints;
    }

    private static async Task<Ok<PokemonContract[]>> GetPokemonsCatalogAsync(
        IQueryHandler<GetPokemonsCatalogQuery, IReadOnlyCollection<PokemonResponse>> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(new GetPokemonsCatalogQuery(), cancellationToken);
        return TypedResults.Ok(response.Select(item => item.ToContract()).ToArray());
    }
}
