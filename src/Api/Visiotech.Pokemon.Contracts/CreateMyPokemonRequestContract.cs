namespace Visiotech.Pokemon.Contracts;

public sealed record CreateMyPokemonRequestContract(
    Guid PokemonSpeciesId,
    int Level,
    int CurrentHealthPoints,
    int TotalHealthPoints,
    IReadOnlyCollection<Guid>? EquippedMoveIds);
