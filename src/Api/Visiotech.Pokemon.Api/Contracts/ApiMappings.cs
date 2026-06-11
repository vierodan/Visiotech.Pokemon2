using Visiotech.Pokemon.Application.Features.System.Queries.GetSystemInfo;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Contracts;

namespace Visiotech.Pokemon.Api.Contracts;

public static class ApiMappings
{
    public static SystemInfoContract ToContract(this SystemInfoResponse response) =>
        new(response.Service, response.Environment, response.Version, response.GeneratedAtUtc);

    public static BattleContract ToContract(this BattleResponse response) =>
        new(
            response.Id,
            response.Status,
            response.CurrentTurnNumber,
            response.NextAttackerMyPokemonId,
            response.WinnerMyPokemonId,
            response.LoserMyPokemonId,
            response.Combatants.Select(item => item.ToContract()).ToArray(),
            response.History.Select(item => item.ToContract()).ToArray());

    public static BattlePhaseExecutionContract ToContract(this BattlePhaseExecutionResponse response) =>
        new(
            response.Battle.ToContract(),
            response.DamageCalculation.ToContract());

    public static MoveDamageCalculationContract ToContract(this MoveDamageCalculationResponse response) =>
        new(
            response.AttackerMyPokemonId,
            response.DefenderMyPokemonId,
            response.MoveId,
            response.MoveName,
            response.MoveType,
            response.MoveCategory,
            response.AttackerLevel,
            response.MovePower,
            response.OffensiveStat,
            response.OffensiveStatValue,
            response.DefensiveStat,
            response.DefensiveStatValue,
            response.DefenderCurrentHealthPoints,
            response.RandomFactor,
            response.BaseDamage,
            response.EffectivenessBreakdown.Select(item => item.ToContract()).ToArray(),
            response.TotalEffectiveness,
            response.RawDamage,
            response.Damage,
            response.DefenderRemainingHealthPoints);

    public static PokemonSpeciesContract ToContract(this PokemonSpeciesResponse response) =>
        new(
            response.Id,
            response.Name,
            response.Types,
            new PokemonBaseStatsContract(
                response.BaseStats.Health,
                response.BaseStats.Attack,
                response.BaseStats.Defense,
                response.BaseStats.SpecialAttack,
                response.BaseStats.SpecialDefense,
                response.BaseStats.Speed));

    public static MyPokemonContract ToContract(this MyPokemonResponse response) =>
        new(
            response.Id,
            response.Species.ToContract(),
            response.Level,
            response.CurrentHealthPoints,
            response.TotalHealthPoints,
            response.EquippedMoves.Select(item => item.ToContract()).ToArray());

    public static MyPokemonCatalogContract ToContract(this MyPokemonCatalogResponse response) =>
        new(
            response.Items.Select(item => item.ToContract()).ToArray(),
            response.Page,
            response.PageSize,
            response.TotalCount,
            response.TotalPages);

    public static MyPokemonEquippedMovesContract ToContract(this MyPokemonEquippedMovesResponse response) =>
        new(
            response.MyPokemonId,
            response.Moves.Select(item => item.ToContract()).ToArray());

    public static MoveDamageCalculationEffectivenessContract ToContract(this MoveDamageCalculationEffectivenessResponse response) =>
        new(
            response.DefenderType,
            response.Multiplier);

    public static BattleCombatantContract ToContract(this BattleCombatantResponse response) =>
        new(
            response.SlotNumber,
            response.MyPokemonId,
            response.CurrentHealthPoints,
            response.TotalHealthPoints);

    public static BattlePhaseContract ToContract(this BattlePhaseResponse response) =>
        new(
            response.SequenceNumber,
            response.AttackerMyPokemonId,
            response.DefenderMyPokemonId,
            response.MoveId,
            response.MoveName,
            response.RandomFactor,
            response.EffectivenessBreakdown.Select(item => item.ToContract()).ToArray(),
            response.TotalEffectiveness,
            response.Damage,
            response.AttackerRemainingHealthPoints,
            response.DefenderRemainingHealthPoints);

    public static BattlePhaseEffectivenessContract ToContract(this BattlePhaseEffectivenessResponse response) =>
        new(
            response.DefenderType,
            response.Multiplier);

    public static PokemonMoveContract ToContract(this PokemonMoveResponse response) =>
        new(
            response.Id,
            response.Name,
            response.Type,
            response.Category,
            response.Power);

    public static PokemonLearnableMovesContract ToContract(this PokemonLearnableMovesResponse response) =>
        new(
            response.PokemonSpeciesId,
            response.PokemonSpeciesName,
            response.Moves.Select(item => item.ToContract()).ToArray());

    public static PokemonMoveSharedSpeciesContract ToContract(this PokemonMoveSharedSpeciesResponse response) =>
        new(
            response.PokemonMoveId,
            response.PokemonMoveName,
            response.PokemonSpecies.Select(item => item.ToContract()).ToArray());

    public static PokemonMoveCatalogContract ToContract(this PokemonMoveCatalogResponse response) =>
        new(
            response.Items.Select(item => item.ToContract()).ToArray(),
            response.Page,
            response.PageSize,
            response.TotalCount,
            response.TotalPages);

    public static PokemonSpeciesCatalogContract ToContract(this PokemonSpeciesCatalogResponse response) =>
        new(
            response.Items.Select(item => item.ToContract()).ToArray(),
            response.Page,
            response.PageSize,
            response.TotalCount,
            response.TotalPages);
}
