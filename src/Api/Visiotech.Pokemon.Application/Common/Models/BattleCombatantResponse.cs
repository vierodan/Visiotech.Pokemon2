namespace Visiotech.Pokemon.Application.Common.Models;

public sealed record BattleCombatantResponse(
    int SlotNumber,
    Guid MyPokemonId,
    int CurrentHealthPoints,
    int TotalHealthPoints);
