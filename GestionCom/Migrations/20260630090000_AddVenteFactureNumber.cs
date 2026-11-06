using GestionCom.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionCom.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260630090000_AddVenteFactureNumber")]
    public partial class AddVenteFactureNumber : Migration
    {
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

            migrationBuilder.Sql(@"
UPDATE ""Ventes""
SET ""FactureNumber"" = ""DocumentNumber""
WHERE COALESCE(""FactureNumber"", '') = '';
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
