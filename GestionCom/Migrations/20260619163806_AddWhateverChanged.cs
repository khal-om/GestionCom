using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionCom.Migrations
{
    /// <inheritdoc />
    public partial class AddWhateverChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                table: "Ventes",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PaiementEtat",
                table: "Ventes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VenteId",
                table: "Payments",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_VenteId",
                table: "Payments",
                column: "VenteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Ventes_VenteId",
                table: "Payments",
                column: "VenteId",
                principalTable: "Ventes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Ventes_VenteId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_VenteId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaidAmount",
                table: "Ventes");

            migrationBuilder.DropColumn(
                name: "PaiementEtat",
                table: "Ventes");

            migrationBuilder.DropColumn(
                name: "VenteId",
                table: "Payments");
        }
    }
}
