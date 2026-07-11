using GestionCom.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionCom.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260504120000_AddProductUnitPriceAndVenteDiscounts")]
    public partial class AddProductUnitPriceAndVenteDiscounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PrixUnitaire",
                table: "Produits",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrixUnitaire",
                table: "VenteLignes",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RemisePercent",
                table: "VenteLignes",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrixUnitaire",
                table: "Produits");

            migrationBuilder.DropColumn(
                name: "PrixUnitaire",
                table: "VenteLignes");

            migrationBuilder.DropColumn(
                name: "RemisePercent",
                table: "VenteLignes");
        }
    }
}
