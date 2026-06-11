namespace Visiotech.Pokemon.Application.Features.Damage;

public sealed record MoveDamageCalculationRequest(
    Guid AttackerMyPokemonId,
    Guid DefenderMyPokemonId,
    Guid MoveId,
    int? AttackerCurrentHealthPointsOverride = null,
    int? DefenderCurrentHealthPointsOverride = null);
