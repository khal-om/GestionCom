using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace GestionCom.Migrations
{
    [DbContext(typeof(GestionCom.Data.AppDbContext))]
    [Migration("20260620120000_AddVentePaymentFields")]
    public partial class AddVentePaymentFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add PaidAmount to Ventes if not exists
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = current_schema()
          AND table_name = 'Ventes'
          AND column_name = 'PaidAmount') THEN
        ALTER TABLE ""Ventes"" ADD COLUMN ""PaidAmount"" numeric NOT NULL DEFAULT 0;
    END IF;
END $$;
            ");

            // Add PaiementEtat to Ventes if not exists
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = current_schema()
          AND table_name = 'Ventes'
          AND column_name = 'PaiementEtat') THEN
        ALTER TABLE ""Ventes"" ADD COLUMN ""PaiementEtat"" integer NOT NULL DEFAULT 0;
    END IF;
END $$;
            ");

            // Add VenteId to Payments and FK if not exists
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = current_schema()
          AND table_name = 'Payments'
          AND column_name = 'VenteId') THEN
        ALTER TABLE ""Payments"" ADD COLUMN ""VenteId"" integer NULL;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM pg_indexes
        WHERE schemaname = current_schema()
          AND tablename = 'Payments'
          AND indexname = 'IX_Payments_VenteId') THEN
        CREATE INDEX ""IX_Payments_VenteId"" ON ""Payments"" (""VenteId"");
    END IF;

    -- Add foreign key if not exists
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu ON tc.constraint_name = kcu.constraint_name
        WHERE tc.table_name = 'Payments' AND tc.constraint_type = 'FOREIGN KEY' AND kcu.column_name = 'VenteId') THEN
        ALTER TABLE ""Payments"" ADD CONSTRAINT ""FK_Payments_Ventes_VenteId"" FOREIGN KEY (""VenteId"") REFERENCES ""Ventes"" (""Id"");
    END IF;
END $$;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu ON tc.constraint_name = kcu.constraint_name
        WHERE tc.table_name = 'Payments' AND tc.constraint_type = 'FOREIGN KEY' AND kcu.column_name = 'VenteId') THEN
        ALTER TABLE ""Payments"" DROP CONSTRAINT ""FK_Payments_Ventes_VenteId"";
    END IF;

    IF EXISTS (
        SELECT 1 FROM pg_indexes
        WHERE schemaname = current_schema()
          AND tablename = 'Payments'
          AND indexname = 'IX_Payments_VenteId') THEN
        DROP INDEX ""IX_Payments_VenteId"";
    END IF;

    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = current_schema()
          AND table_name = 'Payments'
          AND column_name = 'VenteId') THEN
        ALTER TABLE ""Payments"" DROP COLUMN ""VenteId"";
    END IF;

    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = current_schema()
          AND table_name = 'Ventes'
          AND column_name = 'PaiementEtat') THEN
        ALTER TABLE ""Ventes"" DROP COLUMN ""PaiementEtat"";
    END IF;

    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = current_schema()
          AND table_name = 'Ventes'
          AND column_name = 'PaidAmount') THEN
        ALTER TABLE ""Ventes"" DROP COLUMN ""PaidAmount"";
    END IF;
END $$;
            ");
        }
    }
}
