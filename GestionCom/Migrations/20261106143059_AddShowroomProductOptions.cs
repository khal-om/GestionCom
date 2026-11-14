using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GestionCom.Migrations
{
    /// <inheritdoc />
    public partial class AddShowroomProductOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShowroomProductOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OptionType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ValueFr = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    NormalizedValue = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShowroomProductOptions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShowroomProductOptions_OptionType_NormalizedValue",
                table: "ShowroomProductOptions",
                columns: new[] { "OptionType", "NormalizedValue" },
                unique: true);
            // Import existing product sizes into the options table.
            migrationBuilder.Sql("""
                INSERT INTO "ShowroomProductOptions"
                    ("OptionType", "ValueFr", "NormalizedValue", "CreatedAt")
                SELECT 'Size', value, normalized, CURRENT_TIMESTAMP
                FROM (
                    SELECT DISTINCT ON (normalized)
                        value, normalized
                    FROM (
                        SELECT
                            BTRIM("Size") AS value,
                            TRANSLATE(
                                LOWER(BTRIM("Size")),
                                'àáâäãåèéêëìíîïòóôöõùúûüÿç',
                                'aaaaaaeeeeiiiiooooouuuuyc'
                            ) AS normalized
                        FROM "ShowroomProducts"
                        WHERE NULLIF(BTRIM("Size"), '') IS NOT NULL
                    ) AS source
                    ORDER BY normalized, value
                ) AS distinct_values;
                """);

            // Import existing hair types into the options table.
            migrationBuilder.Sql("""
                INSERT INTO "ShowroomProductOptions"
                    ("OptionType", "ValueFr", "NormalizedValue", "CreatedAt")
                SELECT 'HairType', value, normalized, CURRENT_TIMESTAMP
                FROM (
                    SELECT DISTINCT ON (normalized)
                        value, normalized
                    FROM (
                        SELECT
                            BTRIM("HairTypeFr") AS value,
                            TRANSLATE(
                                LOWER(BTRIM("HairTypeFr")),
                                'àáâäãåèéêëìíîïòóôöõùúûüÿç',
                                'aaaaaaeeeeiiiiooooouuuuyc'
                            ) AS normalized
                        FROM "ShowroomProducts"
                        WHERE NULLIF(BTRIM("HairTypeFr"), '') IS NOT NULL
                    ) AS source
                    ORDER BY normalized, value
                ) AS distinct_values;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShowroomProductOptions");
        }
    }
}
