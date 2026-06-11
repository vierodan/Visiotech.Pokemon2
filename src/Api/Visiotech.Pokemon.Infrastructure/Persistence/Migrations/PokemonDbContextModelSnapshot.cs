using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Visiotech.Pokemon.Infrastructure.Persistence;

#nullable disable

namespace Visiotech.Pokemon.Infrastructure.Persistence.Migrations;

[DbContext(typeof(PokemonDbContext))]
partial class PokemonDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "10.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.HasDefaultSchema("pokemon2");

        modelBuilder.Entity("Visiotech.Pokemon.Domain.Battles.Battle", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedNever()
                .HasColumnType("uuid");

            b.Property<int>("CurrentTurnNumber")
                .HasColumnType("integer")
                .HasColumnName("current_turn_number");

            b.Property<Guid>("NextAttackerMyPokemonId")
                .HasColumnType("uuid")
                .HasColumnName("next_attacker_my_pokemon_id");

            b.Property<string>("Status")
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("character varying(20)")
                .HasColumnName("status");

            b.HasKey("Id");

            b.ToTable("battles", "pokemon2", t =>
            {
                t.HasCheckConstraint("ck_battles_current_turn_number", "\"current_turn_number\" >= 1");
                t.HasCheckConstraint("ck_battles_status", "\"status\" IN ('Created', 'InProgress', 'Finished')");
            });
        });

        modelBuilder.Entity("Visiotech.Pokemon.Domain.Battles.BattleCombatant", b =>
        {
            b.Property<Guid>("BattleId")
                .HasColumnType("uuid")
                .HasColumnName("battle_id");

            b.Property<int>("SlotNumber")
                .HasColumnType("integer")
                .HasColumnName("slot_number");

            b.Property<int>("CurrentHealthPoints")
                .HasColumnType("integer")
                .HasColumnName("current_health_points");

            b.Property<Guid>("MyPokemonId")
                .HasColumnType("uuid")
                .HasColumnName("my_pokemon_id");

            b.Property<int>("TotalHealthPoints")
                .HasColumnType("integer")
                .HasColumnName("total_health_points");

            b.HasKey("BattleId", "SlotNumber");

            b.HasIndex("MyPokemonId");

            b.HasIndex("BattleId", "MyPokemonId")
                .IsUnique();

            b.ToTable("battle_combatants", "pokemon2", t =>
            {
                t.HasCheckConstraint("ck_battle_combatants_current_health_non_negative", "\"current_health_points\" >= 0");
                t.HasCheckConstraint("ck_battle_combatants_current_health_range", "\"current_health_points\" <= \"total_health_points\"");
                t.HasCheckConstraint("ck_battle_combatants_slot_number", "\"slot_number\" BETWEEN 1 AND 2");
                t.HasCheckConstraint("ck_battle_combatants_total_health_positive", "\"total_health_points\" > 0");
            });
        });

        modelBuilder.Entity("Visiotech.Pokemon.Domain.Pokemons.MyPokemon", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedNever()
                .HasColumnType("uuid");

            b.Property<int>("CurrentHealthPoints")
                .HasColumnType("integer")
                .HasColumnName("current_health_points");

            b.Property<Guid>("PokemonSpeciesId")
                .HasColumnType("uuid")
                .HasColumnName("pokemon_species_id");

            b.Property<int>("TotalHealthPoints")
                .HasColumnType("integer")
                .HasColumnName("total_health_points");

            b.HasKey("Id");

            b.HasIndex("PokemonSpeciesId");

            b.ToTable("my_pokemons", "pokemon2", t =>
            {
                t.HasCheckConstraint("ck_my_pokemons_current_health_non_negative", "\"current_health_points\" >= 0");
                t.HasCheckConstraint("ck_my_pokemons_current_health_range", "\"current_health_points\" <= \"total_health_points\"");
                t.HasCheckConstraint("ck_my_pokemons_total_health_positive", "\"total_health_points\" > 0");
            });
        });

        modelBuilder.Entity("Visiotech.Pokemon.Domain.Pokemons.MyPokemonMoveSlot", b =>
        {
            b.Property<Guid>("MyPokemonId")
                .HasColumnType("uuid")
                .HasColumnName("my_pokemon_id");

            b.Property<int>("SlotNumber")
                .HasColumnType("integer")
                .HasColumnName("slot_number");

            b.Property<Guid>("PokemonMoveId")
                .HasColumnType("uuid")
                .HasColumnName("pokemon_move_id");

            b.HasKey("MyPokemonId", "SlotNumber");

            b.HasIndex("PokemonMoveId");

            b.HasIndex("MyPokemonId", "PokemonMoveId")
                .IsUnique();

            b.ToTable("my_pokemon_move_slots", "pokemon2", t =>
            {
                t.HasCheckConstraint("ck_my_pokemon_move_slots_slot_number", "\"slot_number\" BETWEEN 1 AND 4");
            });
        });

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

        modelBuilder.Entity("Visiotech.Pokemon.Domain.Battles.BattleCombatant", b =>
        {
            b.HasOne("Visiotech.Pokemon.Domain.Battles.Battle", null)
                .WithMany("Combatants")
                .HasForeignKey("BattleId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            b.HasOne("Visiotech.Pokemon.Domain.Pokemons.MyPokemon", null)
                .WithMany()
                .HasForeignKey("MyPokemonId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
        });

        modelBuilder.Entity("Visiotech.Pokemon.Domain.Pokemons.MyPokemon", b =>
        {
            b.HasOne("Visiotech.Pokemon.Domain.Pokemons.PokemonSpecies", null)
                .WithMany()
                .HasForeignKey("PokemonSpeciesId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
        });

        modelBuilder.Entity("Visiotech.Pokemon.Domain.Battles.Battle", b =>
        {
            b.Navigation("Combatants")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity("Visiotech.Pokemon.Domain.Pokemons.MyPokemonMoveSlot", b =>
        {
            b.HasOne("Visiotech.Pokemon.Domain.Pokemons.MyPokemon", null)
                .WithMany("EquippedMoves")
                .HasForeignKey("MyPokemonId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            b.HasOne("Visiotech.Pokemon.Domain.Pokemons.PokemonMove", null)
                .WithMany()
                .HasForeignKey("PokemonMoveId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
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

        modelBuilder.Entity("Visiotech.Pokemon.Domain.Pokemons.MyPokemon", b =>
        {
            b.OwnsOne("Visiotech.Pokemon.Domain.Pokemons.Level", "Level", b1 =>
            {
                b1.Property<Guid>("MyPokemonId")
                    .HasColumnType("uuid");

                b1.Property<int>("Value")
                    .HasColumnType("integer")
                    .HasColumnName("level");

                b1.HasKey("MyPokemonId");

                b1.ToTable("my_pokemons", "pokemon2");

                b1.WithOwner()
                    .HasForeignKey("MyPokemonId");
            });

            b.Navigation("EquippedMoves")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            b.Navigation("Level")
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
