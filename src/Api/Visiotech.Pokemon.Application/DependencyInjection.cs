using Microsoft.Extensions.DependencyInjection;
using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.Pokemons.Commands.CreatePokemonSpecies;
using Visiotech.Pokemon.Application.Features.Pokemons.Commands.DeletePokemonSpecies;
using Visiotech.Pokemon.Application.Features.Pokemons.Commands.UpdatePokemonSpecies;
using Visiotech.Pokemon.Application.Features.Pokemons.Queries.GetPokemonSpeciesDetail;
using Visiotech.Pokemon.Application.Features.Pokemons.Queries.GetPokemonsCatalog;
using Visiotech.Pokemon.Application.Features.System.Queries.GetSystemInfo;

namespace Visiotech.Pokemon.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IQueryHandler<GetSystemInfoQuery, SystemInfoResponse>, GetSystemInfoQueryHandler>();
        services.AddScoped<IQueryHandler<GetPokemonsCatalogQuery, PokemonSpeciesCatalogResponse>, GetPokemonsCatalogQueryHandler>();
        services.AddScoped<IQueryHandler<GetPokemonSpeciesDetailQuery, PokemonSpeciesResponse>, GetPokemonSpeciesDetailQueryHandler>();
        services.AddScoped<ICommandHandler<CreatePokemonSpeciesCommand, PokemonSpeciesResponse>, CreatePokemonSpeciesCommandHandler>();
        services.AddScoped<ICommandHandler<DeletePokemonSpeciesCommand, Guid>, DeletePokemonSpeciesCommandHandler>();
        services.AddScoped<ICommandHandler<UpdatePokemonSpeciesCommand, PokemonSpeciesResponse>, UpdatePokemonSpeciesCommandHandler>();

        return services;
    }
}
