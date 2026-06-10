using Visiotech.Pokemon.Domain.Abstractions;

namespace Visiotech.Pokemon.Domain.Pokemons;

public sealed record BaseStats : ValueObject
{
    private BaseStats()
    {
    }

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

    public int Health { get; private set; }
    public int Attack { get; private set; }
    public int Defense { get; private set; }
    public int SpecialAttack { get; private set; }
    public int SpecialDefense { get; private set; }
    public int Speed { get; private set; }

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
