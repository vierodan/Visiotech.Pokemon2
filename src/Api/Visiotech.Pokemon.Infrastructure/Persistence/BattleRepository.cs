using Microsoft.EntityFrameworkCore;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Domain.Battles;

namespace Visiotech.Pokemon.Infrastructure.Persistence;

public sealed class BattleRepository(PokemonDbContext dbContext) : IBattleWriteRepository, IBattleReadRepository
{
    public async Task AddAsync(Battle battle, CancellationToken cancellationToken) =>
        await dbContext.Battles.AddAsync(battle, cancellationToken);

    public async Task<Battle?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.Battles
            .AsNoTracking()
            .Include(battle => battle.Combatants)
            .Include(battle => battle.Phases)
                .ThenInclude(phase => phase.EffectivenessBreakdown)
            .SingleOrDefaultAsync(battle => battle.Id == id, cancellationToken);

    public async Task<Battle?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.Battles
            .Include(battle => battle.Combatants)
            .Include(battle => battle.Phases)
                .ThenInclude(phase => phase.EffectivenessBreakdown)
            .SingleOrDefaultAsync(battle => battle.Id == id, cancellationToken);
}
