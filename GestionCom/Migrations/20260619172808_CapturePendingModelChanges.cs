using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionCom.Migrations
{
    /// <inheritdoc />
    public partial class CapturePendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ensure VenteId column exists, create index and foreign key if necessary (idempotent)
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = current_schema()
          AND table_name = 'Payments'
          AND column_name = 'VenteId') THEN
        ALTER TABLE ""Payments"" ADD COLUMN ""VenteId"" integer;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM pg_indexes
        WHERE schemaname = current_schema()
          AND tablename = 'Payments'
          AND indexname = 'IX_Payments_VenteId') THEN
        CREATE INDEX ""IX_Payments_VenteId"" ON ""Payments"" (""VenteId"");
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu ON tc.constraint_name = kcu.constraint_name
        WHERE tc.table_name = 'Payments' AND tc.constraint_type = 'FOREIGN KEY' AND kcu.column_name = 'VenteId') THEN
        ALTER TABLE ""Payments"" ADD CONSTRAINT ""FK_Payments_Ventes_VenteId"" FOREIGN KEY (""VenteId"") REFERENCES ""Ventes"" (""Id"");
    END IF;
END $$;
            ");
        }

        /// <inheritdoc />
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
END $$;
            ");
        }
    }
}
