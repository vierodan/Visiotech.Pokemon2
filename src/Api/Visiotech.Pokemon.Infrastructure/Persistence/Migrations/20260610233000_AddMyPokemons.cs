using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Visiotech.Pokemon.Infrastructure.Persistence.Migrations;

[DbContext(typeof(PokemonDbContext))]
[Migration("20260610233000_AddMyPokemons")]
public partial class AddMyPokemons : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "my_pokemons",
            schema: "pokemon2",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                pokemon_species_id = table.Column<Guid>(type: "uuid", nullable: false),
                level = table.Column<int>(type: "integer", nullable: false),
                current_health_points = table.Column<int>(type: "integer", nullable: false),
                total_health_points = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_my_pokemons", x => x.Id);
                table.CheckConstraint("ck_my_pokemons_current_health_positive", "\"current_health_points\" > 0");
                table.CheckConstraint("ck_my_pokemons_current_health_range", "\"current_health_points\" <= \"total_health_points\"");
                table.CheckConstraint("ck_my_pokemons_total_health_positive", "\"total_health_points\" > 0");
                table.ForeignKey(
                    name: "FK_my_pokemons_pokemon_species_pokemon_species_id",
                    column: x => x.pokemon_species_id,
                    principalSchema: "pokemon2",
                    principalTable: "pokemon_species",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "my_pokemon_move_slots",
            schema: "pokemon2",
            columns: table => new
            {
                my_pokemon_id = table.Column<Guid>(type: "uuid", nullable: false),
                slot_number = table.Column<int>(type: "integer", nullable: false),
                pokemon_move_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_my_pokemon_move_slots", x => new { x.my_pokemon_id, x.slot_number });
                table.CheckConstraint("ck_my_pokemon_move_slots_slot_number", "\"slot_number\" BETWEEN 1 AND 4");
                table.ForeignKey(
                    name: "FK_my_pokemon_move_slots_my_pokemons_my_pokemon_id",
                    column: x => x.my_pokemon_id,
                    principalSchema: "pokemon2",
                    principalTable: "my_pokemons",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_my_pokemon_move_slots_pokemon_moves_pokemon_move_id",
                    column: x => x.pokemon_move_id,
                    principalSchema: "pokemon2",
                    principalTable: "pokemon_moves",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_my_pokemon_move_slots_my_pokemon_id_pokemon_move_id",
            schema: "pokemon2",
            table: "my_pokemon_move_slots",
            columns: new[] { "my_pokemon_id", "pokemon_move_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_my_pokemon_move_slots_pokemon_move_id",
            schema: "pokemon2",
            table: "my_pokemon_move_slots",
            column: "pokemon_move_id");

        migrationBuilder.CreateIndex(
            name: "IX_my_pokemons_pokemon_species_id",
            schema: "pokemon2",
            table: "my_pokemons",
            column: "pokemon_species_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "my_pokemon_move_slots",
            schema: "pokemon2");

        migrationBuilder.DropTable(
            name: "my_pokemons",
            schema: "pokemon2");
    }
}
