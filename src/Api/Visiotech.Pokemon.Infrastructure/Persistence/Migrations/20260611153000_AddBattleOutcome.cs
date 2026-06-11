using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Visiotech.Pokemon.Infrastructure.Persistence.Migrations;

[DbContext(typeof(PokemonDbContext))]
[Migration("20260611153000_AddBattleOutcome")]
public partial class AddBattleOutcome : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<Guid>(
            name: "next_attacker_my_pokemon_id",
            schema: "pokemon2",
            table: "battles",
            type: "uuid",
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uuid");

        migrationBuilder.AddColumn<Guid>(
            name: "winner_my_pokemon_id",
            schema: "pokemon2",
            table: "battles",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "loser_my_pokemon_id",
            schema: "pokemon2",
            table: "battles",
            type: "uuid",
            nullable: true);

        migrationBuilder.Sql("""
            UPDATE "pokemon2"."battles" b
            SET "winner_my_pokemon_id" = winning.my_pokemon_id,
                "loser_my_pokemon_id" = losing.my_pokemon_id,
                "next_attacker_my_pokemon_id" = NULL
            FROM "pokemon2"."battle_combatants" winning
            INNER JOIN "pokemon2"."battle_combatants" losing
                ON losing.battle_id = winning.battle_id
               AND losing.my_pokemon_id <> winning.my_pokemon_id
            WHERE b."Id" = winning.battle_id
              AND b.status = 'Finished'
              AND winning.current_health_points > 0
              AND losing.current_health_points = 0;
            """);

        migrationBuilder.AddCheckConstraint(
            name: "ck_battles_outcome_consistency",
            schema: "pokemon2",
            table: "battles",
            sql: "(\"winner_my_pokemon_id\" IS NULL AND \"loser_my_pokemon_id\" IS NULL) OR (\"winner_my_pokemon_id\" IS NOT NULL AND \"loser_my_pokemon_id\" IS NOT NULL AND \"winner_my_pokemon_id\" <> \"loser_my_pokemon_id\")");

        migrationBuilder.AddCheckConstraint(
            name: "ck_battles_turn_state_consistency",
            schema: "pokemon2",
            table: "battles",
            sql: "(\"status\" = 'Finished' AND \"next_attacker_my_pokemon_id\" IS NULL AND \"winner_my_pokemon_id\" IS NOT NULL AND \"loser_my_pokemon_id\" IS NOT NULL) OR (\"status\" IN ('Created', 'InProgress') AND \"next_attacker_my_pokemon_id\" IS NOT NULL AND \"winner_my_pokemon_id\" IS NULL AND \"loser_my_pokemon_id\" IS NULL)");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(
            name: "ck_battles_outcome_consistency",
            schema: "pokemon2",
            table: "battles");

        migrationBuilder.DropCheckConstraint(
            name: "ck_battles_turn_state_consistency",
            schema: "pokemon2",
            table: "battles");

        migrationBuilder.Sql("""
            UPDATE "pokemon2"."battles"
            SET "next_attacker_my_pokemon_id" = COALESCE("winner_my_pokemon_id", "next_attacker_my_pokemon_id");
            """);

        migrationBuilder.DropColumn(
            name: "winner_my_pokemon_id",
            schema: "pokemon2",
            table: "battles");

        migrationBuilder.DropColumn(
            name: "loser_my_pokemon_id",
            schema: "pokemon2",
            table: "battles");

        migrationBuilder.AlterColumn<Guid>(
            name: "next_attacker_my_pokemon_id",
            schema: "pokemon2",
            table: "battles",
            type: "uuid",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "uuid",
            oldNullable: true);
    }
}
