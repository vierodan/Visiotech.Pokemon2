using Visiotech.Pokemon.Domain.Abstractions;

namespace Visiotech.Pokemon.Domain.Pokemons;

public sealed record PokemonTyping : ValueObject
{
    private PokemonTyping()
    {
    }

    private PokemonTyping(PokemonType primaryType, PokemonType? secondaryType)
    {
        PrimaryType = primaryType;
        SecondaryType = secondaryType;
    }

    public PokemonType PrimaryType { get; private set; }
    public PokemonType? SecondaryType { get; private set; }

    public IReadOnlyCollection<PokemonType> Types =>
        SecondaryType is null
            ? [PrimaryType]
            : [PrimaryType, SecondaryType.Value];

    public static PokemonTyping Create(IEnumerable<PokemonType> types)
    {
        ArgumentNullException.ThrowIfNull(types);

        var normalizedTypes = types.ToArray();
        if (normalizedTypes.Length is < 1 or > 2)
        {
            throw new DomainException("Pokemon species must have between 1 and 2 types.");
        }

        if (normalizedTypes.Distinct().Count() != normalizedTypes.Length)
        {
            throw new DomainException("Pokemon species types must be unique.");
        }

        return new PokemonTyping(
            normalizedTypes[0],
            normalizedTypes.Length == 2 ? normalizedTypes[1] : null);
    }
}
