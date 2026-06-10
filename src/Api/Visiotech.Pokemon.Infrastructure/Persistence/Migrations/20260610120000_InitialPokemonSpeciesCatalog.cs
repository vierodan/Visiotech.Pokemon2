using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Visiotech.Pokemon.Infrastructure.Persistence.Migrations;

public partial class InitialPokemonSpeciesCatalog : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "catalog");

        migrationBuilder.CreateTable(
            name: "pokemon_species",
            schema: "catalog",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                normalized_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                name_normalized_value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                primary_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                secondary_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                health = table.Column<int>(type: "integer", nullable: false),
                attack = table.Column<int>(type: "integer", nullable: false),
                defense = table.Column<int>(type: "integer", nullable: false),
                special_attack = table.Column<int>(type: "integer", nullable: false),
                special_defense = table.Column<int>(type: "integer", nullable: false),
                speed = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_pokemon_species", x => x.Id);
                table.CheckConstraint("ck_pokemon_species_attack_positive", "\"attack\" > 0");
                table.CheckConstraint("ck_pokemon_species_defense_positive", "\"defense\" > 0");
                table.CheckConstraint("ck_pokemon_species_health_positive", "\"health\" > 0");
                table.CheckConstraint("ck_pokemon_species_secondary_type", "\"secondary_type\" IS NULL OR \"secondary_type\" <> \"primary_type\"");
                table.CheckConstraint("ck_pokemon_species_special_attack_positive", "\"special_attack\" > 0");
                table.CheckConstraint("ck_pokemon_species_special_defense_positive", "\"special_defense\" > 0");
                table.CheckConstraint("ck_pokemon_species_speed_positive", "\"speed\" > 0");
            });

        migrationBuilder.CreateIndex(
            name: "IX_pokemon_species_normalized_name",
            schema: "catalog",
            table: "pokemon_species",
            column: "normalized_name",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "pokemon_species",
            schema: "catalog");
    }
}
