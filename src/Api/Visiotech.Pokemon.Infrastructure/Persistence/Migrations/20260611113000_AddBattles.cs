using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Visiotech.Pokemon.Infrastructure.Persistence.Migrations;

[DbContext(typeof(PokemonDbContext))]
[Migration("20260611113000_AddBattles")]
public partial class AddBattles : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(
            name: "ck_my_pokemons_current_health_positive",
            schema: "pokemon2",
            table: "my_pokemons");

        migrationBuilder.AddCheckConstraint(
            name: "ck_my_pokemons_current_health_non_negative",
            schema: "pokemon2",
            table: "my_pokemons",
            sql: "\"current_health_points\" >= 0");

        migrationBuilder.CreateTable(
            name: "battles",
            schema: "pokemon2",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                current_turn_number = table.Column<int>(type: "integer", nullable: false),
                next_attacker_my_pokemon_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_battles", x => x.Id);
                table.CheckConstraint("ck_battles_current_turn_number", "\"current_turn_number\" >= 1");
                table.CheckConstraint("ck_battles_status", "\"status\" IN ('Created', 'InProgress', 'Finished')");
            });

        migrationBuilder.CreateTable(
            name: "battle_combatants",
            schema: "pokemon2",
            columns: table => new
            {
                battle_id = table.Column<Guid>(type: "uuid", nullable: false),
                slot_number = table.Column<int>(type: "integer", nullable: false),
                my_pokemon_id = table.Column<Guid>(type: "uuid", nullable: false),
                current_health_points = table.Column<int>(type: "integer", nullable: false),
                total_health_points = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_battle_combatants", x => new { x.battle_id, x.slot_number });
                table.CheckConstraint("ck_battle_combatants_current_health_non_negative", "\"current_health_points\" >= 0");
                table.CheckConstraint("ck_battle_combatants_current_health_range", "\"current_health_points\" <= \"total_health_points\"");
                table.CheckConstraint("ck_battle_combatants_slot_number", "\"slot_number\" BETWEEN 1 AND 2");
                table.CheckConstraint("ck_battle_combatants_total_health_positive", "\"total_health_points\" > 0");
                table.ForeignKey(
                    name: "FK_battle_combatants_battles_battle_id",
                    column: x => x.battle_id,
                    principalSchema: "pokemon2",
                    principalTable: "battles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_battle_combatants_my_pokemons_my_pokemon_id",
                    column: x => x.my_pokemon_id,
                    principalSchema: "pokemon2",
                    principalTable: "my_pokemons",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_battle_combatants_battle_id_my_pokemon_id",
            schema: "pokemon2",
            table: "battle_combatants",
            columns: new[] { "battle_id", "my_pokemon_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_battle_combatants_my_pokemon_id",
            schema: "pokemon2",
            table: "battle_combatants",
            column: "my_pokemon_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "battle_combatants",
            schema: "pokemon2");

        migrationBuilder.DropTable(
            name: "battles",
            schema: "pokemon2");

        migrationBuilder.DropCheckConstraint(
            name: "ck_my_pokemons_current_health_non_negative",
            schema: "pokemon2",
            table: "my_pokemons");

        migrationBuilder.AddCheckConstraint(
            name: "ck_my_pokemons_current_health_positive",
            schema: "pokemon2",
            table: "my_pokemons",
            sql: "\"current_health_points\" > 0");
    }
}
