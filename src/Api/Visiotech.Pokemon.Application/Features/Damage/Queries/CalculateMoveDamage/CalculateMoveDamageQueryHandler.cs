using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Abstractions.Randomization;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Domain.Abstractions;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Application.Features.Damage.Queries.CalculateMoveDamage;

public sealed class CalculateMoveDamageQueryHandler(
    IMyPokemonReadRepository myPokemonRepository,
    IPokemonSpeciesReadRepository speciesRepository,
    IPokemonMoveReadRepository moveRepository,
    IDamageRandomProvider damageRandomProvider)
    : IQueryHandler<CalculateMoveDamageQuery, MoveDamageCalculationResponse>
{
    public async Task<MoveDamageCalculationResponse> Handle(
        CalculateMoveDamageQuery query,
        CancellationToken cancellationToken)
    {
        var errors = Validate(query);
        if (errors.Count > 0)
        {
            throw new ApplicationValidationException(errors);
        }

        var attacker = await myPokemonRepository.GetByIdAsync(query.AttackerMyPokemonId, cancellationToken);
        if (attacker is null)
        {
            throw new ApplicationNotFoundException(
                $"My pokemon '{query.AttackerMyPokemonId}' was not found.",
                "attackerMyPokemonId");
        }

        var defender = await myPokemonRepository.GetByIdAsync(query.DefenderMyPokemonId, cancellationToken);
        if (defender is null)
        {
            throw new ApplicationNotFoundException(
                $"My pokemon '{query.DefenderMyPokemonId}' was not found.",
                "defenderMyPokemonId");
        }

        EnsureCombatReady(attacker, "attackerMyPokemonId");
        EnsureCombatReady(defender, "defenderMyPokemonId");

        if (!attacker.EquippedMoveIds.Contains(query.MoveId))
        {
            throw new ApplicationValidationException(new Dictionary<string, string[]>
            {
                ["moveId"] = [$"Pokemon move '{query.MoveId}' is not equipped by my pokemon '{attacker.Id}'."]
            });
        }

        var move = await moveRepository.GetByIdAsync(query.MoveId, cancellationToken);
        if (move is null)
        {
            throw new ApplicationNotFoundException(
                $"Pokemon move '{query.MoveId}' was not found.",
                "moveId");
        }

        if (move.Category is MoveCategory.Status)
        {
            throw new ApplicationValidationException(new Dictionary<string, string[]>
            {
                ["moveId"] = [$"Pokemon move '{move.Name.Value}' cannot be used to calculate damage because it is a Status move."]
            });
        }

        var attackerSpecies = await speciesRepository.GetByIdAsync(attacker.PokemonSpeciesId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Pokemon species '{attacker.PokemonSpeciesId}' referenced by my pokemon '{attacker.Id}' was not found.");

        var defenderSpecies = await speciesRepository.GetByIdAsync(defender.PokemonSpeciesId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Pokemon species '{defender.PokemonSpeciesId}' referenced by my pokemon '{defender.Id}' was not found.");

        DamageCalculationResult calculation;

        try
        {
            calculation = MoveDamageCalculator.Calculate(new DamageCalculationInput(
                attacker.Level.Value,
                attackerSpecies.BaseStats,
                defenderSpecies.BaseStats,
                defender.CurrentHealthPoints,
                move.Type,
                move.Category,
                move.Power,
                defenderSpecies.Types.ToArray(),
                damageRandomProvider.Next()));
        }
        catch (DomainException exception)
        {
            throw new ApplicationValidationException(new Dictionary<string, string[]>
            {
                ["damageCalculation"] = [exception.Message]
            });
        }

        return new MoveDamageCalculationResponse(
            attacker.Id,
            defender.Id,
            move.Id,
            move.Name.Value,
            move.Type.ToString(),
            move.Category.ToString(),
            attacker.Level.Value,
            move.Power,
            calculation.OffensiveStat,
            calculation.OffensiveStatValue,
            calculation.DefensiveStat,
            calculation.DefensiveStatValue,
            defender.CurrentHealthPoints,
            calculation.RandomFactor,
            calculation.BaseDamage,
            calculation.EffectivenessBreakdown
                .Select(item => new MoveDamageCalculationEffectivenessResponse(item.DefenderType.ToString(), item.Multiplier))
                .ToArray(),
            calculation.TotalEffectiveness,
            calculation.RawDamage,
            calculation.Damage,
            calculation.DefenderRemainingHealthPoints);
    }

    private static IReadOnlyDictionary<string, string[]> Validate(CalculateMoveDamageQuery query)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (query.AttackerMyPokemonId == Guid.Empty)
        {
            errors["attackerMyPokemonId"] = ["AttackerMyPokemonId is required."];
        }

        if (query.DefenderMyPokemonId == Guid.Empty)
        {
            errors["defenderMyPokemonId"] = ["DefenderMyPokemonId is required."];
        }

        if (query.MoveId == Guid.Empty)
        {
            errors["moveId"] = ["MoveId is required."];
        }

        return errors;
    }

    private static void EnsureCombatReady(MyPokemon myPokemon, string target)
    {
        if (myPokemon.CurrentHealthPoints <= 0)
        {
            throw new ApplicationValidationException(new Dictionary<string, string[]>
            {
                [target] = [$"My pokemon '{myPokemon.Id}' must have current health points greater than 0 to calculate damage."]
            });
        }
    }
}
