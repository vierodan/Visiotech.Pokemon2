using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Visiotech.Pokemon.Infrastructure.Persistence.Migrations;

public partial class AddPokemonMovesCatalog : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "pokemon_moves",
            schema: "pokemon2",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                normalized_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                power = table.Column<int>(type: "integer", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                name_normalized_value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_pokemon_moves", x => x.Id);
                table.CheckConstraint("ck_pokemon_moves_category", "\"category\" IN ('Physical', 'Special', 'Status')");
                table.CheckConstraint(
                    "ck_pokemon_moves_power_by_category",
                    "(\"category\" = 'Status' AND \"power\" = 0) OR (\"category\" IN ('Physical', 'Special') AND \"power\" > 0)");
            });

        migrationBuilder.CreateIndex(
            name: "IX_pokemon_moves_normalized_name",
            schema: "pokemon2",
            table: "pokemon_moves",
            column: "normalized_name",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "pokemon_moves",
            schema: "pokemon2");
    }
}
