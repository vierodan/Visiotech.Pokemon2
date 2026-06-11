using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Abstractions.Randomization;
using Visiotech.Pokemon.Application.Abstractions.Services;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Domain.Abstractions;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Application.Features.Damage;

public sealed class MoveDamageCalculationService(
    IMyPokemonReadRepository myPokemonRepository,
    IPokemonSpeciesReadRepository speciesRepository,
    IPokemonMoveReadRepository moveRepository,
    IDamageRandomProvider damageRandomProvider)
    : IMoveDamageCalculationService
{
    public async Task<MoveDamageCalculationResponse> CalculateAsync(
        MoveDamageCalculationRequest request,
        CancellationToken cancellationToken)
    {
        var errors = Validate(request);
        if (errors.Count > 0)
        {
            throw new ApplicationValidationException(errors);
        }

        var attacker = await myPokemonRepository.GetByIdAsync(request.AttackerMyPokemonId, cancellationToken);
        if (attacker is null)
        {
            throw new ApplicationNotFoundException(
                $"My pokemon '{request.AttackerMyPokemonId}' was not found.",
                "attackerMyPokemonId");
        }

        var defender = await myPokemonRepository.GetByIdAsync(request.DefenderMyPokemonId, cancellationToken);
        if (defender is null)
        {
            throw new ApplicationNotFoundException(
                $"My pokemon '{request.DefenderMyPokemonId}' was not found.",
                "defenderMyPokemonId");
        }

        var attackerCurrentHealthPoints = request.AttackerCurrentHealthPointsOverride ?? attacker.CurrentHealthPoints;
        var defenderCurrentHealthPoints = request.DefenderCurrentHealthPointsOverride ?? defender.CurrentHealthPoints;

        EnsureCombatReady(attackerCurrentHealthPoints, attacker.Id, "attackerMyPokemonId");
        EnsureCombatReady(defenderCurrentHealthPoints, defender.Id, "defenderMyPokemonId");

        if (!attacker.EquippedMoveIds.Contains(request.MoveId))
        {
            throw new ApplicationValidationException(new Dictionary<string, string[]>
            {
                ["moveId"] = [$"Pokemon move '{request.MoveId}' is not equipped by my pokemon '{attacker.Id}'."]
            });
        }

        var move = await moveRepository.GetByIdAsync(request.MoveId, cancellationToken);
        if (move is null)
        {
            throw new ApplicationNotFoundException(
                $"Pokemon move '{request.MoveId}' was not found.",
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
                defenderCurrentHealthPoints,
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
            defenderCurrentHealthPoints,
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

    private static IReadOnlyDictionary<string, string[]> Validate(MoveDamageCalculationRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (request.AttackerMyPokemonId == Guid.Empty)
        {
            errors["attackerMyPokemonId"] = ["AttackerMyPokemonId is required."];
        }

        if (request.DefenderMyPokemonId == Guid.Empty)
        {
            errors["defenderMyPokemonId"] = ["DefenderMyPokemonId is required."];
        }

        if (request.MoveId == Guid.Empty)
        {
            errors["moveId"] = ["MoveId is required."];
        }

        if (request.AttackerCurrentHealthPointsOverride is < 0)
        {
            errors["attackerMyPokemonId"] = ["Attacker current health points override cannot be negative."];
        }

        if (request.DefenderCurrentHealthPointsOverride is < 0)
        {
            errors["defenderMyPokemonId"] = ["Defender current health points override cannot be negative."];
        }

        return errors;
    }

    private static void EnsureCombatReady(int currentHealthPoints, Guid myPokemonId, string target)
    {
        if (currentHealthPoints <= 0)
        {
            throw new ApplicationValidationException(new Dictionary<string, string[]>
            {
                [target] = [$"My pokemon '{myPokemonId}' must have current health points greater than 0 to calculate damage."]
            });
        }
    }
}
