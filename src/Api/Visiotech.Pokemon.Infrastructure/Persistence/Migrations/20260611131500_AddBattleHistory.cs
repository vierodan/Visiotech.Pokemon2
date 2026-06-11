using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Visiotech.Pokemon.Infrastructure.Persistence.Migrations;

[DbContext(typeof(PokemonDbContext))]
[Migration("20260611131500_AddBattleHistory")]
public partial class AddBattleHistory : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "battle_phases",
            schema: "pokemon2",
            columns: table => new
            {
                battle_id = table.Column<Guid>(type: "uuid", nullable: false),
                sequence_number = table.Column<int>(type: "integer", nullable: false),
                attacker_my_pokemon_id = table.Column<Guid>(type: "uuid", nullable: false),
                defender_my_pokemon_id = table.Column<Guid>(type: "uuid", nullable: false),
                move_id = table.Column<Guid>(type: "uuid", nullable: false),
                move_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                random_factor = table.Column<int>(type: "integer", nullable: false),
                total_effectiveness = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                damage = table.Column<int>(type: "integer", nullable: false),
                attacker_remaining_health_points = table.Column<int>(type: "integer", nullable: false),
                defender_remaining_health_points = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_battle_phases", x => new { x.battle_id, x.sequence_number });
                table.CheckConstraint("ck_battle_phases_attacker_remaining_health_non_negative", "\"attacker_remaining_health_points\" >= 0");
                table.CheckConstraint("ck_battle_phases_damage_non_negative", "\"damage\" >= 0");
                table.CheckConstraint("ck_battle_phases_defender_remaining_health_non_negative", "\"defender_remaining_health_points\" >= 0");
                table.CheckConstraint("ck_battle_phases_random_factor", "\"random_factor\" BETWEEN 85 AND 100");
                table.CheckConstraint("ck_battle_phases_sequence_number", "\"sequence_number\" >= 1");
                table.ForeignKey(
                    name: "FK_battle_phases_battles_battle_id",
                    column: x => x.battle_id,
                    principalSchema: "pokemon2",
                    principalTable: "battles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "battle_phase_effectiveness",
            schema: "pokemon2",
            columns: table => new
            {
                battle_id = table.Column<Guid>(type: "uuid", nullable: false),
                battle_phase_sequence_number = table.Column<int>(type: "integer", nullable: false),
                defender_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                multiplier = table.Column<decimal>(type: "numeric(5,2)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_battle_phase_effectiveness", x => new
                {
                    x.battle_id,
                    x.battle_phase_sequence_number,
                    x.defender_type
                });
                table.CheckConstraint("ck_battle_phase_effectiveness_multiplier_non_negative", "\"multiplier\" >= 0");
                table.ForeignKey(
                    name: "FK_battle_phase_effectiveness_battle_phases_battle_id_battle_phase_sequence_number",
                    columns: x => new { x.battle_id, x.battle_phase_sequence_number },
                    principalSchema: "pokemon2",
                    principalTable: "battle_phases",
                    principalColumns: new[] { "battle_id", "sequence_number" },
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_battle_phases_attacker_my_pokemon_id",
            schema: "pokemon2",
            table: "battle_phases",
            column: "attacker_my_pokemon_id");

        migrationBuilder.CreateIndex(
            name: "IX_battle_phases_defender_my_pokemon_id",
            schema: "pokemon2",
            table: "battle_phases",
            column: "defender_my_pokemon_id");

        migrationBuilder.CreateIndex(
            name: "IX_battle_phases_move_id",
            schema: "pokemon2",
            table: "battle_phases",
            column: "move_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "battle_phase_effectiveness",
            schema: "pokemon2");

        migrationBuilder.DropTable(
            name: "battle_phases",
            schema: "pokemon2");
    }
}
