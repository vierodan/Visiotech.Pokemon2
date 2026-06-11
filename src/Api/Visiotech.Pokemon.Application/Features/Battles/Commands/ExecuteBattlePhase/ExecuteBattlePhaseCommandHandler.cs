using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Abstractions.Services;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.Damage;
using Visiotech.Pokemon.Domain.Abstractions;
using Visiotech.Pokemon.Domain.Battles;

namespace Visiotech.Pokemon.Application.Features.Battles.Commands.ExecuteBattlePhase;

public sealed class ExecuteBattlePhaseCommandHandler(
    IBattleWriteRepository battleRepository,
    IMyPokemonWriteRepository myPokemonRepository,
    IMoveDamageCalculationService moveDamageCalculationService,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ExecuteBattlePhaseCommand, BattlePhaseExecutionResponse>
{
    public async Task<BattlePhaseExecutionResponse> Handle(
        ExecuteBattlePhaseCommand command,
        CancellationToken cancellationToken)
    {
        var errors = Validate(command);
        if (errors.Count > 0)
        {
            throw new ApplicationValidationException(errors);
        }

        var battle = await battleRepository.GetForUpdateAsync(command.BattleId, cancellationToken);
        if (battle is null)
        {
            throw new ApplicationNotFoundException(
                $"Battle '{command.BattleId}' was not found.",
                "id");
        }

        if (battle.Status == BattleStatus.Finished)
        {
            throw new ApplicationValidationException(new Dictionary<string, string[]>
            {
                ["id"] = [$"Battle '{battle.Id}' is already finished and cannot execute additional phases."]
            });
        }

        var attackerCombatant = battle.Combatants.SingleOrDefault(item => item.MyPokemonId == command.AttackerMyPokemonId);
        if (attackerCombatant is null)
        {
            throw new ApplicationValidationException(new Dictionary<string, string[]>
            {
                ["attackerMyPokemonId"] = [$"My pokemon '{command.AttackerMyPokemonId}' does not belong to battle '{battle.Id}'."]
            });
        }

        if (battle.NextAttackerMyPokemonId != command.AttackerMyPokemonId)
        {
            throw new ApplicationValidationException(new Dictionary<string, string[]>
            {
                ["attackerMyPokemonId"] = [$"My pokemon '{command.AttackerMyPokemonId}' is not the next attacker for battle '{battle.Id}'."]
            });
        }

        if (attackerCombatant.CurrentHealthPoints <= 0)
        {
            throw new ApplicationValidationException(new Dictionary<string, string[]>
            {
                ["attackerMyPokemonId"] = [$"My pokemon '{command.AttackerMyPokemonId}' must have current health points greater than 0 to execute a battle phase."]
            });
        }

        var defenderCombatant = battle.Combatants.SingleOrDefault(item => item.MyPokemonId != command.AttackerMyPokemonId)
            ?? throw new InvalidOperationException($"Battle '{battle.Id}' does not contain a defender combatant.");

        if (defenderCombatant.CurrentHealthPoints <= 0)
        {
            throw new ApplicationValidationException(new Dictionary<string, string[]>
            {
                ["id"] = [$"Battle '{battle.Id}' cannot continue because the defender already has 0 current health points."]
            });
        }

        var attacker = await myPokemonRepository.GetForUpdateAsync(attackerCombatant.MyPokemonId, cancellationToken)
            ?? throw new ApplicationNotFoundException(
                $"My pokemon '{attackerCombatant.MyPokemonId}' referenced by battle '{battle.Id}' was not found.",
                "attackerMyPokemonId");

        var defender = await myPokemonRepository.GetForUpdateAsync(defenderCombatant.MyPokemonId, cancellationToken)
            ?? throw new ApplicationNotFoundException(
                $"My pokemon '{defenderCombatant.MyPokemonId}' referenced by battle '{battle.Id}' was not found.",
                "defenderMyPokemonId");

        var damageCalculation = await moveDamageCalculationService.CalculateAsync(
            new MoveDamageCalculationRequest(
                attacker.Id,
                defender.Id,
                command.MoveId,
                attackerCombatant.CurrentHealthPoints,
                defenderCombatant.CurrentHealthPoints),
            cancellationToken);

        var battleContinues = damageCalculation.DefenderRemainingHealthPoints > 0;
        var sequenceNumber = battle.CurrentTurnNumber;

        try
        {
            battle.RecordPhase(new BattlePhaseRegistration(
                sequenceNumber,
                attackerCombatant.MyPokemonId,
                defenderCombatant.MyPokemonId,
                damageCalculation.MoveId,
                damageCalculation.MoveName,
                damageCalculation.RandomFactor,
                damageCalculation.EffectivenessBreakdown
                    .Select(item => new BattlePhaseEffectivenessInput(
                        Enum.Parse<Domain.Pokemons.PokemonType>(item.DefenderType, ignoreCase: false),
                        item.Multiplier))
                    .ToArray(),
                damageCalculation.TotalEffectiveness,
                damageCalculation.Damage,
                attackerCombatant.CurrentHealthPoints,
                damageCalculation.DefenderRemainingHealthPoints,
                battleContinues ? defenderCombatant.MyPokemonId : null,
                !battleContinues));
        }
        catch (DomainException exception)
        {
            throw new ApplicationValidationException(new Dictionary<string, string[]>
            {
                ["battle"] = [exception.Message]
            });
        }

        var updatedAttacker = battle.Combatants.Single(item => item.MyPokemonId == attackerCombatant.MyPokemonId);
        var updatedDefender = battle.Combatants.Single(item => item.MyPokemonId == defenderCombatant.MyPokemonId);

        attacker.UpdateCurrentHealthPoints(updatedAttacker.CurrentHealthPoints);
        defender.UpdateCurrentHealthPoints(updatedDefender.CurrentHealthPoints);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new BattlePhaseExecutionResponse(
            BattleMapping.ToResponse(battle),
            damageCalculation);
    }

    private static IReadOnlyDictionary<string, string[]> Validate(ExecuteBattlePhaseCommand command)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (command.BattleId == Guid.Empty)
        {
            errors["id"] = ["Id is required."];
        }

        if (command.AttackerMyPokemonId == Guid.Empty)
        {
            errors["attackerMyPokemonId"] = ["AttackerMyPokemonId is required."];
        }

        if (command.MoveId == Guid.Empty)
        {
            errors["moveId"] = ["MoveId is required."];
        }

        return errors;
    }
}
