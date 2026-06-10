using Visiotech.Pokemon.Domain.Abstractions;

namespace Visiotech.Pokemon.Domain.Pokemons;

public sealed class PokemonLearnableMove
{
    private PokemonLearnableMove()
    {
    }

    private PokemonLearnableMove(Guid pokemonSpeciesId, Guid pokemonMoveId)
    {
        PokemonSpeciesId = pokemonSpeciesId;
        PokemonMoveId = pokemonMoveId;
    }

    public Guid PokemonSpeciesId { get; private set; }

    public Guid PokemonMoveId { get; private set; }

    public static PokemonLearnableMove Create(Guid pokemonSpeciesId, Guid pokemonMoveId)
    {
        if (pokemonSpeciesId == Guid.Empty)
        {
            throw new DomainException("Pokemon species id cannot be empty.");
        }

        if (pokemonMoveId == Guid.Empty)
        {
            throw new DomainException("Pokemon move id cannot be empty.");
        }

        return new PokemonLearnableMove(pokemonSpeciesId, pokemonMoveId);
    }
}
