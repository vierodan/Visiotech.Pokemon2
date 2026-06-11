namespace Visiotech.Pokemon.Contracts;

public sealed record CalculateMoveDamageRequestContract(
    Guid AttackerMyPokemonId,
    Guid DefenderMyPokemonId,
    Guid MoveId);
