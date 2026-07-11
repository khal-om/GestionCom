using GestionCom.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionCom.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260504123000_FixMissingProductAndVenteLinePricingColumns")]
    public partial class FixMissingProductAndVenteLinePricingColumns : Migration
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
        WHERE table_schema = current_schema()
          AND table_name = 'Produits'
          AND column_name = 'PrixUnitaire') THEN
        ALTER TABLE ""Produits"" ADD COLUMN ""PrixUnitaire"" numeric(18,2) NULL;
    END IF;
END $$;");

            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = current_schema()
          AND table_name = 'VenteLignes'
          AND column_name = 'PrixUnitaire') THEN
        ALTER TABLE ""VenteLignes"" ADD COLUMN ""PrixUnitaire"" numeric(18,2) NULL;
    END IF;
END $$;");

            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = current_schema()
          AND table_name = 'VenteLignes'
          AND column_name = 'RemisePercent') THEN
        ALTER TABLE ""VenteLignes"" ADD COLUMN ""RemisePercent"" numeric(5,2) NOT NULL DEFAULT 0;
    END IF;
END $$;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = current_schema()
          AND table_name = 'VenteLignes'
          AND column_name = 'RemisePercent') THEN
        ALTER TABLE ""VenteLignes"" DROP COLUMN ""RemisePercent"";
    END IF;
END $$;");

            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = current_schema()
          AND table_name = 'VenteLignes'
          AND column_name = 'PrixUnitaire') THEN
        ALTER TABLE ""VenteLignes"" DROP COLUMN ""PrixUnitaire"";
    END IF;
END $$;");

            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = current_schema()
          AND table_name = 'Produits'
          AND column_name = 'PrixUnitaire') THEN
        ALTER TABLE ""Produits"" DROP COLUMN ""PrixUnitaire"";
    END IF;
END $$;");
        }
    }
}
