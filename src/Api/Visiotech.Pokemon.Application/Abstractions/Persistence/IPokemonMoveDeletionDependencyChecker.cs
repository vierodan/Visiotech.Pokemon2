namespace Visiotech.Pokemon.Application.Abstractions.Persistence;

public interface IPokemonMoveDeletionDependencyChecker
{
    Task<IReadOnlyCollection<string>> GetBlockingReasonsAsync(Guid pokemonMoveId, CancellationToken cancellationToken);
}
