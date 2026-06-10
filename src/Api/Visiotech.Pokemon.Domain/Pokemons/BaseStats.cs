using Visiotech.Pokemon.Domain.Abstractions;

namespace Visiotech.Pokemon.Domain.Pokemons;

public sealed record BaseStats : ValueObject
{
    private BaseStats(
        int health,
        int attack,
        int defense,
        int specialAttack,
        int specialDefense,
        int speed)
    {
        Health = health;
        Attack = attack;
        Defense = defense;
        SpecialAttack = specialAttack;
        SpecialDefense = specialDefense;
        Speed = speed;
    }

    public int Health { get; }
    public int Attack { get; }
    public int Defense { get; }
    public int SpecialAttack { get; }
    public int SpecialDefense { get; }
    public int Speed { get; }

    public static BaseStats Create(
        int health,
        int attack,
        int defense,
        int specialAttack,
        int specialDefense,
        int speed)
    {
        EnsurePositive(health, nameof(health));
        EnsurePositive(attack, nameof(attack));
        EnsurePositive(defense, nameof(defense));
        EnsurePositive(specialAttack, nameof(specialAttack));
        EnsurePositive(specialDefense, nameof(specialDefense));
        EnsurePositive(speed, nameof(speed));

        return new BaseStats(health, attack, defense, specialAttack, specialDefense, speed);
    }

    private static void EnsurePositive(int value, string name)
    {
        if (value <= 0)
        {
            throw new DomainException($"{name} must be greater than zero.");
        }
    }
}
