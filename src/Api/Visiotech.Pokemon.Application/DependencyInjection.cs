using Microsoft.Extensions.DependencyInjection;
using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Common.Observability;
using Visiotech.Pokemon.Application.Features.Battles.Commands.CreateBattle;
using Visiotech.Pokemon.Application.Features.Damage.Queries.CalculateMoveDamage;
using Visiotech.Pokemon.Application.Features.MyPokemons.Commands.CreateMyPokemon;
using Visiotech.Pokemon.Application.Features.MyPokemons.Commands.DeleteMyPokemon;
using Visiotech.Pokemon.Application.Features.MyPokemons.Queries.GetMyPokemonEquippedMoves;
using Visiotech.Pokemon.Application.Features.MyPokemons.Commands.UpdateMyPokemon;
using Visiotech.Pokemon.Application.Features.MyPokemons.Queries.GetMyPokemonDetail;
using Visiotech.Pokemon.Application.Features.MyPokemons.Queries.GetMyPokemonsCatalog;
using Visiotech.Pokemon.Application.Features.Moves.Commands.CreatePokemonMove;
using Visiotech.Pokemon.Application.Features.Moves.Commands.DeletePokemonMove;
using Visiotech.Pokemon.Application.Features.Moves.Commands.UpdatePokemonMove;
using Visiotech.Pokemon.Application.Features.Moves.Queries.GetPokemonMoveDetail;
using Visiotech.Pokemon.Application.Features.Moves.Queries.GetPokemonMovesCatalog;
using Visiotech.Pokemon.Application.Features.Moves.Queries.GetPokemonMoveSharedSpecies;
using Visiotech.Pokemon.Application.Features.Pokemons.Commands.CreatePokemonSpecies;
using Visiotech.Pokemon.Application.Features.Pokemons.Commands.DeletePokemonSpecies;
using Visiotech.Pokemon.Application.Features.Pokemons.Commands.UpdatePokemonSpeciesLearnableMoves;
using Visiotech.Pokemon.Application.Features.Pokemons.Commands.UpdatePokemonSpecies;
using Visiotech.Pokemon.Application.Features.Pokemons.Queries.GetPokemonSpeciesLearnableMoves;
using Visiotech.Pokemon.Application.Features.Pokemons.Queries.GetPokemonSpeciesDetail;
using Visiotech.Pokemon.Application.Features.Pokemons.Queries.GetPokemonsCatalog;
using Visiotech.Pokemon.Application.Features.System.Queries.GetSystemInfo;

namespace Visiotech.Pokemon.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        AddLoggedCommandHandler<CreateBattleCommand, BattleResponse, CreateBattleCommandHandler>(services);
        AddLoggedQueryHandler<CalculateMoveDamageQuery, MoveDamageCalculationResponse, CalculateMoveDamageQueryHandler>(services);
        AddLoggedQueryHandler<GetSystemInfoQuery, SystemInfoResponse, GetSystemInfoQueryHandler>(services);
        AddLoggedQueryHandler<GetPokemonMovesCatalogQuery, PokemonMoveCatalogResponse, GetPokemonMovesCatalogQueryHandler>(services);
        AddLoggedQueryHandler<GetPokemonMoveDetailQuery, PokemonMoveResponse, GetPokemonMoveDetailQueryHandler>(services);
        AddLoggedQueryHandler<GetPokemonMoveSharedSpeciesQuery, PokemonMoveSharedSpeciesResponse, GetPokemonMoveSharedSpeciesQueryHandler>(services);
        AddLoggedQueryHandler<GetPokemonsCatalogQuery, PokemonSpeciesCatalogResponse, GetPokemonsCatalogQueryHandler>(services);
        AddLoggedQueryHandler<GetPokemonSpeciesDetailQuery, PokemonSpeciesResponse, GetPokemonSpeciesDetailQueryHandler>(services);
        AddLoggedQueryHandler<GetPokemonSpeciesLearnableMovesQuery, PokemonLearnableMovesResponse, GetPokemonSpeciesLearnableMovesQueryHandler>(services);
        AddLoggedQueryHandler<GetMyPokemonsCatalogQuery, MyPokemonCatalogResponse, GetMyPokemonsCatalogQueryHandler>(services);
        AddLoggedQueryHandler<GetMyPokemonDetailQuery, MyPokemonResponse, GetMyPokemonDetailQueryHandler>(services);
        AddLoggedQueryHandler<GetMyPokemonEquippedMovesQuery, MyPokemonEquippedMovesResponse, GetMyPokemonEquippedMovesQueryHandler>(services);
        AddLoggedCommandHandler<CreateMyPokemonCommand, MyPokemonResponse, CreateMyPokemonCommandHandler>(services);
        AddLoggedCommandHandler<DeleteMyPokemonCommand, Guid, DeleteMyPokemonCommandHandler>(services);
        AddLoggedCommandHandler<UpdateMyPokemonCommand, MyPokemonResponse, UpdateMyPokemonCommandHandler>(services);
        AddLoggedCommandHandler<CreatePokemonMoveCommand, PokemonMoveResponse, CreatePokemonMoveCommandHandler>(services);
        AddLoggedCommandHandler<DeletePokemonMoveCommand, Guid, DeletePokemonMoveCommandHandler>(services);
        AddLoggedCommandHandler<UpdatePokemonMoveCommand, PokemonMoveResponse, UpdatePokemonMoveCommandHandler>(services);
        AddLoggedCommandHandler<CreatePokemonSpeciesCommand, PokemonSpeciesResponse, CreatePokemonSpeciesCommandHandler>(services);
        AddLoggedCommandHandler<DeletePokemonSpeciesCommand, Guid, DeletePokemonSpeciesCommandHandler>(services);
        AddLoggedCommandHandler<UpdatePokemonSpeciesLearnableMovesCommand, PokemonLearnableMovesResponse, UpdatePokemonSpeciesLearnableMovesCommandHandler>(services);
        AddLoggedCommandHandler<UpdatePokemonSpeciesCommand, PokemonSpeciesResponse, UpdatePokemonSpeciesCommandHandler>(services);

        return services;
    }

    private static void AddLoggedCommandHandler<TCommand, TResponse, THandler>(IServiceCollection services)
        where TCommand : class, ICommand<TResponse>
        where THandler : class, ICommandHandler<TCommand, TResponse>
    {
        services.AddScoped<THandler>();
        services.AddScoped<ICommandHandler<TCommand, TResponse>>(serviceProvider =>
            new LoggingCommandHandler<TCommand, TResponse>(
                serviceProvider.GetRequiredService<THandler>(),
                serviceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<LoggingCommandHandler<TCommand, TResponse>>>()));
    }

    private static void AddLoggedQueryHandler<TQuery, TResponse, THandler>(IServiceCollection services)
        where TQuery : class, IQuery<TResponse>
        where THandler : class, IQueryHandler<TQuery, TResponse>
    {
        services.AddScoped<THandler>();
        services.AddScoped<IQueryHandler<TQuery, TResponse>>(serviceProvider =>
            new LoggingQueryHandler<TQuery, TResponse>(
                serviceProvider.GetRequiredService<THandler>(),
                serviceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<LoggingQueryHandler<TQuery, TResponse>>>()));
    }
}
