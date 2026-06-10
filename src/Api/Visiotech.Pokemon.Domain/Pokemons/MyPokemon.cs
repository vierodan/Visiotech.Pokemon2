using Visiotech.Pokemon.Domain.Abstractions;

namespace Visiotech.Pokemon.Domain.Pokemons;

public sealed class MyPokemon : AggregateRoot<Guid>
{
    private readonly List<MyPokemonMoveSlot> _equippedMoves = [];

    private MyPokemon()
    {
        Level = null!;
    }

    private MyPokemon(
        Guid id,
        Guid pokemonSpeciesId,
        Level level,
        int currentHealthPoints,
        int totalHealthPoints,
        IReadOnlyCollection<Guid> equippedMoveIds)
        : base(id)
    {
        PokemonSpeciesId = pokemonSpeciesId;
        Level = level;
        RehydrateBattleState(currentHealthPoints, totalHealthPoints);
        EquipMoves(equippedMoveIds);
    }

    public Guid PokemonSpeciesId { get; private set; }
    public Level Level { get; private set; }
    public int CurrentHealthPoints { get; private set; }
    public int TotalHealthPoints { get; private set; }
    public IReadOnlyCollection<MyPokemonMoveSlot> EquippedMoves => _equippedMoves.AsReadOnly();
    public IReadOnlyCollection<Guid> EquippedMoveIds => _equippedMoves
        .OrderBy(slot => slot.SlotNumber)
        .Select(slot => slot.PokemonMoveId)
        .ToArray();

    public static MyPokemon Create(
        Guid id,
        Guid pokemonSpeciesId,
        Level level,
        int currentHealthPoints,
        int totalHealthPoints,
        IReadOnlyCollection<Guid> equippedMoveIds)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("My pokemon id cannot be empty.");
        }

        if (pokemonSpeciesId == Guid.Empty)
        {
            throw new DomainException("Pokemon species id cannot be empty.");
        }

        return new MyPokemon(id, pokemonSpeciesId, level, currentHealthPoints, totalHealthPoints, equippedMoveIds);
    }

    public void UpdateBattleState(int currentHealthPoints, int totalHealthPoints) =>
        RehydrateBattleState(currentHealthPoints, totalHealthPoints);

    public void ReconfigureLevel(Level level) => Level = level;

    public void ReplaceEquippedMoves(IReadOnlyCollection<Guid> equippedMoveIds)
    {
        _equippedMoves.Clear();
        EquipMoves(equippedMoveIds);
    }

    private void RehydrateBattleState(int currentHealthPoints, int totalHealthPoints)
    {
        if (totalHealthPoints <= 0)
        {
            throw new DomainException("Pokemon total health points must be greater than 0.");
        }

        if (currentHealthPoints <= 0)
        {
            throw new DomainException("Pokemon current health points must be greater than 0.");
        }

        if (currentHealthPoints > totalHealthPoints)
        {
            throw new DomainException("Pokemon current health points cannot exceed total health points.");
        }

        CurrentHealthPoints = currentHealthPoints;
        TotalHealthPoints = totalHealthPoints;
    }

    private void EquipMoves(IReadOnlyCollection<Guid> equippedMoveIds)
    {
        if (equippedMoveIds.Count is < 1 or > 4)
        {
            throw new DomainException("My pokemon must equip between 1 and 4 moves.");
        }

        if (equippedMoveIds.Any(moveId => moveId == Guid.Empty))
        {
            throw new DomainException("Equipped move ids cannot contain empty values.");
        }

        if (equippedMoveIds.Count != equippedMoveIds.Distinct().Count())
        {
            throw new DomainException("My pokemon cannot equip the same move more than once.");
        }

        var slotNumber = 1;
        foreach (var moveId in equippedMoveIds)
        {
            _equippedMoves.Add(MyPokemonMoveSlot.Create(Id, slotNumber, moveId));
            slotNumber++;
        }
    }
}
