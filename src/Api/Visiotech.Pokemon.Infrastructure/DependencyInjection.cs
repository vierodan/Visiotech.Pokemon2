using Microsoft.Extensions.DependencyInjection;
using Visiotech.Pokemon.Application.Abstractions.Clock;
using Visiotech.Pokemon.Infrastructure.Clock;
using Visiotech.Pokemon.Infrastructure.Persistence.InMemory;
using Visiotech.Pokemon.Application.Abstractions.Persistence;

namespace Visiotech.Pokemon.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IPokemonReadRepository, InMemoryPokemonReadRepository>();

        return services;
    }
}
