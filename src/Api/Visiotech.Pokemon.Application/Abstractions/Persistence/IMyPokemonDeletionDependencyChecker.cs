namespace Visiotech.Pokemon.Application.Abstractions.Persistence;

public interface IMyPokemonDeletionDependencyChecker
{
    Task<IReadOnlyCollection<string>> GetBlockingReasonsAsync(Guid myPokemonId, CancellationToken cancellationToken);
}
