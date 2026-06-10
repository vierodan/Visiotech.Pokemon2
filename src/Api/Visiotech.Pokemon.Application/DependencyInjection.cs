using Microsoft.Extensions.DependencyInjection;
using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.System.Queries.GetSystemInfo;
using Visiotech.Pokemon.Application.Features.Pokemons.Queries.GetPokemonsCatalog;

namespace Visiotech.Pokemon.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IQueryHandler<GetSystemInfoQuery, SystemInfoResponse>, GetSystemInfoQueryHandler>();
        services.AddScoped<IQueryHandler<GetPokemonsCatalogQuery, IReadOnlyCollection<PokemonResponse>>, GetPokemonsCatalogQueryHandler>();

        return services;
    }
}
