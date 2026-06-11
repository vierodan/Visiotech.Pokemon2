using Visiotech.Pokemon.Domain.Battles;

namespace Visiotech.Pokemon.Application.Abstractions.Persistence;

public interface IBattleWriteRepository
{
    Task AddAsync(Battle battle, CancellationToken cancellationToken);

    Task<Battle?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken);
}
