namespace Visiotech.Pokemon.Application.Abstractions.Persistence;

public interface IPokemonSpeciesDeletionDependencyChecker
{
    Task<IReadOnlyCollection<string>> GetBlockingReasonsAsync(Guid pokemonSpeciesId, CancellationToken cancellationToken);
}
