using Visiotech.Pokemon.Domain.Abstractions;

namespace Visiotech.Pokemon.Domain.Pokemons;

public sealed class MyPokemonMoveSlot
{
    private MyPokemonMoveSlot()
    {
    }

    private MyPokemonMoveSlot(Guid myPokemonId, int slotNumber, Guid pokemonMoveId)
    {
        MyPokemonId = myPokemonId;
        SlotNumber = slotNumber;
        PokemonMoveId = pokemonMoveId;
    }

    public Guid MyPokemonId { get; private set; }
    public int SlotNumber { get; private set; }
    public Guid PokemonMoveId { get; private set; }

    public void Reassign(int slotNumber, Guid pokemonMoveId)
    {
        if (slotNumber is < 1 or > 4)
        {
            throw new DomainException("Move slot number must be between 1 and 4.");
        }

        if (pokemonMoveId == Guid.Empty)
        {
            throw new DomainException("Pokemon move id cannot be empty.");
        }

        SlotNumber = slotNumber;
        PokemonMoveId = pokemonMoveId;
    }

    public static MyPokemonMoveSlot Create(Guid myPokemonId, int slotNumber, Guid pokemonMoveId)
    {
        if (myPokemonId == Guid.Empty)
        {
            throw new DomainException("My pokemon id cannot be empty.");
        }

        if (slotNumber is < 1 or > 4)
        {
            throw new DomainException("Move slot number must be between 1 and 4.");
        }

        if (pokemonMoveId == Guid.Empty)
        {
            throw new DomainException("Pokemon move id cannot be empty.");
        }

        return new MyPokemonMoveSlot(myPokemonId, slotNumber, pokemonMoveId);
    }
}
