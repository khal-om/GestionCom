using System;
using GestionCom.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionCom.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260428120000_AddVenteDocumentNumber")]
    public partial class AddVenteDocumentNumber : Migration
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
          AND table_name = 'Ventes'
          AND column_name = 'DocumentNumber') THEN
        ALTER TABLE ""Ventes"" ADD COLUMN ""DocumentNumber"" text NOT NULL DEFAULT '';
    END IF;
END $$;");

            migrationBuilder.Sql(@"
                UPDATE ""Ventes""
                SET ""DocumentNumber"" = 'V-' || TO_CHAR(COALESCE(""DateVente"", NOW()), 'DD-MM-YY') || '-' || UPPER(SUBSTRING(MD5(""Id""::text || '-' || COALESCE(""CreatedAt""::text, NOW()::text)) FROM 1 FOR 5))
                WHERE COALESCE(""DocumentNumber"", '') = '';
            ");

            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_indexes
        WHERE schemaname = current_schema()
          AND tablename = 'Ventes'
          AND indexname = 'IX_Ventes_DocumentNumber') THEN
        CREATE UNIQUE INDEX ""IX_Ventes_DocumentNumber"" ON ""Ventes"" (""DocumentNumber"");
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
        FROM pg_indexes
        WHERE schemaname = current_schema()
          AND tablename = 'Ventes'
          AND indexname = 'IX_Ventes_DocumentNumber') THEN
        DROP INDEX ""IX_Ventes_DocumentNumber"";
    END IF;
END $$;");

            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = current_schema()
          AND table_name = 'Ventes'
          AND column_name = 'DocumentNumber') THEN
        ALTER TABLE ""Ventes"" DROP COLUMN ""DocumentNumber"";
    END IF;
END $$;");
        }
    }
}
