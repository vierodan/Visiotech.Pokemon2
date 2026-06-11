using Visiotech.Pokemon.Domain.Battles;

namespace Visiotech.Pokemon.Application.Abstractions.Persistence;

public interface IBattleReadRepository
{
    Task<Battle?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
