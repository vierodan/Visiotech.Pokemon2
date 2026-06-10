using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Visiotech.Pokemon.Infrastructure.Persistence;

#nullable disable

namespace Visiotech.Pokemon.Infrastructure.Persistence.Migrations;

[DbContext(typeof(PokemonDbContext))]
partial class PokemonDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasDefaultSchema("pokemon2");

        modelBuilder.Entity("Visiotech.Pokemon.Domain.Pokemons.PokemonMove", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedNever()
                .HasColumnType("uuid");

            b.Property<string>("Category")
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("character varying(20)")
                .HasColumnName("category");

            b.Property<string>("NormalizedName")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("character varying(100)")
                .HasColumnName("normalized_name");

            b.Property<int>("Power")
                .HasColumnType("integer")
                .HasColumnName("power");

            b.Property<string>("Type")
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("character varying(20)")
                .HasColumnName("type");

            b.HasKey("Id");

            b.HasIndex("NormalizedName")
                .IsUnique();

            b.ToTable("pokemon_moves", "pokemon2", t =>
            {
                t.HasCheckConstraint("ck_pokemon_moves_category", "\"category\" IN ('Physical', 'Special', 'Status')");
                t.HasCheckConstraint("ck_pokemon_moves_power_by_category", "(\"category\" = 'Status' AND \"power\" = 0) OR (\"category\" IN ('Physical', 'Special') AND \"power\" > 0)");
            });
        });

        modelBuilder.Entity("Visiotech.Pokemon.Domain.Pokemons.PokemonLearnableMove", b =>
        {
            b.Property<Guid>("PokemonSpeciesId")
                .HasColumnType("uuid")
                .HasColumnName("pokemon_species_id");

            b.Property<Guid>("PokemonMoveId")
                .HasColumnType("uuid")
                .HasColumnName("pokemon_move_id");

            b.HasKey("PokemonSpeciesId", "PokemonMoveId");

            b.HasIndex("PokemonMoveId");

            b.ToTable("pokemon_species_learnable_moves", "pokemon2");
        });

        modelBuilder.Entity("Visiotech.Pokemon.Domain.Pokemons.PokemonSpecies", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedNever()
                .HasColumnType("uuid");

            b.Property<string>("NormalizedName")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("character varying(100)")
                .HasColumnName("normalized_name");

            b.HasKey("Id");

            b.HasIndex("NormalizedName")
                .IsUnique();

            b.ToTable("pokemon_species", "pokemon2", t =>
            {
                t.HasCheckConstraint("ck_pokemon_species_attack_positive", "\"attack\" > 0");
                t.HasCheckConstraint("ck_pokemon_species_defense_positive", "\"defense\" > 0");
                t.HasCheckConstraint("ck_pokemon_species_health_positive", "\"health\" > 0");
                t.HasCheckConstraint("ck_pokemon_species_secondary_type", "\"secondary_type\" IS NULL OR \"secondary_type\" <> \"primary_type\"");
                t.HasCheckConstraint("ck_pokemon_species_special_attack_positive", "\"special_attack\" > 0");
                t.HasCheckConstraint("ck_pokemon_species_special_defense_positive", "\"special_defense\" > 0");
                t.HasCheckConstraint("ck_pokemon_species_speed_positive", "\"speed\" > 0");
            });
        });

        modelBuilder.Entity("Visiotech.Pokemon.Domain.Pokemons.PokemonLearnableMove", b =>
        {
            b.HasOne("Visiotech.Pokemon.Domain.Pokemons.PokemonMove", null)
                .WithMany()
                .HasForeignKey("PokemonMoveId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            b.HasOne("Visiotech.Pokemon.Domain.Pokemons.PokemonSpecies", null)
                .WithMany("LearnableMoves")
                .HasForeignKey("PokemonSpeciesId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
        });

        modelBuilder.Entity("Visiotech.Pokemon.Domain.Pokemons.PokemonMove", b =>
        {
            b.OwnsOne("Visiotech.Pokemon.Domain.Pokemons.Name", "Name", b1 =>
            {
                b1.Property<Guid>("PokemonMoveId")
                    .HasColumnType("uuid");

                b1.Property<string>("NormalizedValue")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("character varying(100)")
                    .HasColumnName("name_normalized_value");

                b1.Property<string>("Value")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("character varying(100)")
                    .HasColumnName("name");

                b1.HasKey("PokemonMoveId");

                b1.ToTable("pokemon_moves", "pokemon2");

                b1.WithOwner()
                    .HasForeignKey("PokemonMoveId");
            });

            b.Navigation("Name")
                .IsRequired();
        });

        modelBuilder.Entity("Visiotech.Pokemon.Domain.Pokemons.PokemonSpecies", b =>
        {
            b.OwnsOne("Visiotech.Pokemon.Domain.Pokemons.BaseStats", "BaseStats", b1 =>
            {
                b1.Property<Guid>("PokemonSpeciesId")
                    .HasColumnType("uuid");

                b1.Property<int>("Attack")
                    .HasColumnType("integer")
                    .HasColumnName("attack");

                b1.Property<int>("Defense")
                    .HasColumnType("integer")
                    .HasColumnName("defense");

                b1.Property<int>("Health")
                    .HasColumnType("integer")
                    .HasColumnName("health");

                b1.Property<int>("SpecialAttack")
                    .HasColumnType("integer")
                    .HasColumnName("special_attack");

                b1.Property<int>("SpecialDefense")
                    .HasColumnType("integer")
                    .HasColumnName("special_defense");

                b1.Property<int>("Speed")
                    .HasColumnType("integer")
                    .HasColumnName("speed");

                b1.HasKey("PokemonSpeciesId");

                b1.ToTable("pokemon_species", "pokemon2");

                b1.WithOwner()
                    .HasForeignKey("PokemonSpeciesId");
            });

            b.OwnsOne("Visiotech.Pokemon.Domain.Pokemons.Name", "Name", b1 =>
            {
                b1.Property<Guid>("PokemonSpeciesId")
                    .HasColumnType("uuid");

                b1.Property<string>("NormalizedValue")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("character varying(100)")
                    .HasColumnName("name_normalized_value");

                b1.Property<string>("Value")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("character varying(100)")
                    .HasColumnName("name");

                b1.HasKey("PokemonSpeciesId");

                b1.ToTable("pokemon_species", "pokemon2");

                b1.WithOwner()
                    .HasForeignKey("PokemonSpeciesId");
            });

            b.OwnsOne("Visiotech.Pokemon.Domain.Pokemons.PokemonTyping", "Typing", b1 =>
            {
                b1.Property<Guid>("PokemonSpeciesId")
                    .HasColumnType("uuid");

                b1.Property<string>("PrimaryType")
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnType("character varying(20)")
                    .HasColumnName("primary_type");

                b1.Property<string>("SecondaryType")
                    .HasMaxLength(20)
                    .HasColumnType("character varying(20)")
                    .HasColumnName("secondary_type");

                b1.HasKey("PokemonSpeciesId");

                b1.ToTable("pokemon_species", "pokemon2");

                b1.WithOwner()
                    .HasForeignKey("PokemonSpeciesId");
            });

            b.Navigation("BaseStats")
                .IsRequired();

            b.Navigation("Name")
                .IsRequired();

            b.Navigation("Typing")
                .IsRequired();

            b.Navigation("LearnableMoves")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });
    }
}
