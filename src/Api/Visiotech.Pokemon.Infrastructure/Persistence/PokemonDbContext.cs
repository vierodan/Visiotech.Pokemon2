using Microsoft.EntityFrameworkCore;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Infrastructure.Persistence;

public sealed class PokemonDbContext(DbContextOptions<PokemonDbContext> options) : DbContext(options)
{
    public DbSet<PokemonLearnableMove> PokemonLearnableMoves => Set<PokemonLearnableMove>();
    public DbSet<PokemonMove> PokemonMoves => Set<PokemonMove>();
    public DbSet<PokemonSpecies> PokemonSpecies => Set<PokemonSpecies>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("pokemon2");

        modelBuilder.Entity<PokemonMove>(entity =>
        {
            entity.ToTable("pokemon_moves", tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "ck_pokemon_moves_category",
                    "\"category\" IN ('Physical', 'Special', 'Status')");
                tableBuilder.HasCheckConstraint(
                    "ck_pokemon_moves_power_by_category",
                    "(\"category\" = 'Status' AND \"power\" = 0) OR (\"category\" IN ('Physical', 'Special') AND \"power\" > 0)");
            });

            entity.HasKey(move => move.Id);
            entity.Property(move => move.Id).ValueGeneratedNever();

            entity.Property(move => move.NormalizedName)
                .HasColumnName("normalized_name")
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(move => move.NormalizedName)
                .IsUnique();

            entity.OwnsOne(move => move.Name, name =>
            {
                name.Property(value => value.Value)
                    .HasColumnName("name")
                    .HasMaxLength(100)
                    .IsRequired();

                name.Property(value => value.NormalizedValue)
                    .HasColumnName("name_normalized_value")
                    .HasMaxLength(100)
                    .IsRequired();
            });

            entity.Property(move => move.Type)
                .HasColumnName("type")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(move => move.Category)
                .HasColumnName("category")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(move => move.Power)
                .HasColumnName("power")
                .IsRequired();

            entity.Navigation(move => move.Name).IsRequired();
        });

        modelBuilder.Entity<PokemonSpecies>(entity =>
        {
            entity.ToTable("pokemon_species", tableBuilder =>
            {
                tableBuilder.HasCheckConstraint("ck_pokemon_species_secondary_type", "\"secondary_type\" IS NULL OR \"secondary_type\" <> \"primary_type\"");
                tableBuilder.HasCheckConstraint("ck_pokemon_species_health_positive", "\"health\" > 0");
                tableBuilder.HasCheckConstraint("ck_pokemon_species_attack_positive", "\"attack\" > 0");
                tableBuilder.HasCheckConstraint("ck_pokemon_species_defense_positive", "\"defense\" > 0");
                tableBuilder.HasCheckConstraint("ck_pokemon_species_special_attack_positive", "\"special_attack\" > 0");
                tableBuilder.HasCheckConstraint("ck_pokemon_species_special_defense_positive", "\"special_defense\" > 0");
                tableBuilder.HasCheckConstraint("ck_pokemon_species_speed_positive", "\"speed\" > 0");
            });

            entity.HasKey(species => species.Id);
            entity.Property(species => species.Id).ValueGeneratedNever();

            entity.Property(species => species.NormalizedName)
                .HasColumnName("normalized_name")
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(species => species.NormalizedName)
                .IsUnique();

            entity.OwnsOne(species => species.Name, name =>
            {
                name.Property(value => value.Value)
                    .HasColumnName("name")
                    .HasMaxLength(100)
                    .IsRequired();

                name.Property(value => value.NormalizedValue)
                    .HasColumnName("name_normalized_value")
                    .HasMaxLength(100)
                    .IsRequired();
            });

            entity.OwnsOne(species => species.Typing, typing =>
            {
                typing.Property(value => value.PrimaryType)
                    .HasColumnName("primary_type")
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired();

                typing.Property(value => value.SecondaryType)
                    .HasColumnName("secondary_type")
                    .HasConversion<string>()
                    .HasMaxLength(20);
            });

            entity.OwnsOne(species => species.BaseStats, baseStats =>
            {
                baseStats.Property(value => value.Health).HasColumnName("health").IsRequired();
                baseStats.Property(value => value.Attack).HasColumnName("attack").IsRequired();
                baseStats.Property(value => value.Defense).HasColumnName("defense").IsRequired();
                baseStats.Property(value => value.SpecialAttack).HasColumnName("special_attack").IsRequired();
                baseStats.Property(value => value.SpecialDefense).HasColumnName("special_defense").IsRequired();
                baseStats.Property(value => value.Speed).HasColumnName("speed").IsRequired();
            });

            entity.Navigation(species => species.Name).IsRequired();
            entity.Navigation(species => species.Typing).IsRequired();
            entity.Navigation(species => species.BaseStats).IsRequired();
            entity.Navigation(species => species.LearnableMoves)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<PokemonLearnableMove>(entity =>
        {
            entity.ToTable("pokemon_species_learnable_moves");

            entity.HasKey(learnableMove => new { learnableMove.PokemonSpeciesId, learnableMove.PokemonMoveId });

            entity.Property(learnableMove => learnableMove.PokemonSpeciesId)
                .HasColumnName("pokemon_species_id")
                .IsRequired();

            entity.Property(learnableMove => learnableMove.PokemonMoveId)
                .HasColumnName("pokemon_move_id")
                .IsRequired();

            entity.HasOne<PokemonSpecies>()
                .WithMany(species => species.LearnableMoves)
                .HasForeignKey(learnableMove => learnableMove.PokemonSpeciesId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<PokemonMove>()
                .WithMany()
                .HasForeignKey(learnableMove => learnableMove.PokemonMoveId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(learnableMove => learnableMove.PokemonMoveId);
        });
    }
}
