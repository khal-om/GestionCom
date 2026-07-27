using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GestionCom.Migrations
{
    /// <inheritdoc />
    public partial class AddSavingEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name = 'Ventes'
          AND column_name = 'FactureNumber') THEN
        ALTER TABLE ""Ventes"" ADD COLUMN ""FactureNumber"" text NULL;
    END IF;
END $$;
");

            migrationBuilder.CreateTable(
                name: "SavingEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavingEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SavingEntries_Date",
                table: "SavingEntries",
                column: "Date",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SavingEntries");

            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name = 'Ventes'
          AND column_name = 'FactureNumber') THEN
        ALTER TABLE ""Ventes"" DROP COLUMN ""FactureNumber"";
    END IF;
END $$;
");
        }
    }
}
