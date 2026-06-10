using Visiotech.Pokemon.Domain.Abstractions;

namespace Visiotech.Pokemon.Domain.Pokemons;

public sealed class PokemonMove : AggregateRoot<Guid>
{
    private PokemonMove()
    {
        Name = null!;
        NormalizedName = string.Empty;
    }

    private PokemonMove(Guid id, Move move)
        : base(id)
    {
        Name = null!;
        NormalizedName = string.Empty;
        Apply(move);
    }

    public Name Name { get; private set; }
    public string NormalizedName { get; private set; }
    public PokemonType Type { get; private set; }
    public MoveCategory Category { get; private set; }
    public int Power { get; private set; }

    public static PokemonMove Create(Guid id, Move move)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Pokemon move id cannot be empty.");
        }

        return new PokemonMove(id, move);
    }

    public Move ToValueObject() => Move.Create(Name, Type, Category, Power);

    public void Reconfigure(Move move) => Apply(move);

    private void Apply(Move move)
    {
        Name = move.Name;
        NormalizedName = move.Name.NormalizedValue;
        Type = move.Type;
        Category = move.Category;
        Power = move.Power;
    }
}
