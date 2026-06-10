using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Visiotech.Pokemon.Infrastructure.Persistence.Migrations;

public partial class AddPokemonSpeciesLearnableMoves : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "pokemon_species_learnable_moves",
            schema: "catalog",
            columns: table => new
            {
                pokemon_species_id = table.Column<Guid>(type: "uuid", nullable: false),
                pokemon_move_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_pokemon_species_learnable_moves", x => new { x.pokemon_species_id, x.pokemon_move_id });
                table.ForeignKey(
                    name: "FK_pokemon_species_learnable_moves_pokemon_moves_pokemon_move_id",
                    column: x => x.pokemon_move_id,
                    principalSchema: "catalog",
                    principalTable: "pokemon_moves",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_pokemon_species_learnable_moves_pokemon_species_pokemon_species_id",
                    column: x => x.pokemon_species_id,
                    principalSchema: "catalog",
                    principalTable: "pokemon_species",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_pokemon_species_learnable_moves_pokemon_move_id",
            schema: "catalog",
            table: "pokemon_species_learnable_moves",
            column: "pokemon_move_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "pokemon_species_learnable_moves",
            schema: "catalog");
    }
}
