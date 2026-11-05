using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionCom.Migrations
{
    /// <inheritdoc />
    public partial class RemoveArabicShowroomFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ButtonLabelAr",
                table: "ShowroomServices");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "ShowroomServices");

            migrationBuilder.DropColumn(
                name: "ShortDescriptionAr",
                table: "ShowroomServices");

            migrationBuilder.DropColumn(
                name: "TitleAr",
                table: "ShowroomServices");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "ShowroomRoutines");

            migrationBuilder.DropColumn(
                name: "HairNeedAr",
                table: "ShowroomRoutines");

            migrationBuilder.DropColumn(
                name: "TitleAr",
                table: "ShowroomRoutines");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "ShowroomRanges");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "ShowroomRanges");

            migrationBuilder.DropColumn(
                name: "AvailabilityAr",
                table: "ShowroomProducts");

            migrationBuilder.DropColumn(
                name: "CategoryAr",
                table: "ShowroomProducts");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "ShowroomProducts");

            migrationBuilder.DropColumn(
                name: "HairNeedAr",
                table: "ShowroomProducts");

            migrationBuilder.DropColumn(
                name: "HairTypeAr",
                table: "ShowroomProducts");

            migrationBuilder.DropColumn(
                name: "HowToUseAr",
                table: "ShowroomProducts");

            migrationBuilder.DropColumn(
                name: "IngredientsAr",
                table: "ShowroomProducts");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "ShowroomProducts");

            migrationBuilder.DropColumn(
                name: "ShortDescriptionAr",
                table: "ShowroomProducts");

            migrationBuilder.DropColumn(
                name: "LabelAr",
                table: "ShowroomNavigationItems");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "ShowroomBrands");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "ShowroomBrands");

            migrationBuilder.DropColumn(
                name: "TaglineAr",
                table: "ShowroomBrands");

            migrationBuilder.DropColumn(
                name: "BodyAr",
                table: "ShowroomArticles");

            migrationBuilder.DropColumn(
                name: "ExcerptAr",
                table: "ShowroomArticles");

            migrationBuilder.DropColumn(
                name: "TitleAr",
                table: "ShowroomArticles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ButtonLabelAr",
                table: "ShowroomServices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "ShowroomServices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortDescriptionAr",
                table: "ShowroomServices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleAr",
                table: "ShowroomServices",
                type: "character varying(240)",
                maxLength: 240,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "ShowroomRoutines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HairNeedAr",
                table: "ShowroomRoutines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleAr",
                table: "ShowroomRoutines",
                type: "character varying(240)",
                maxLength: 240,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "ShowroomRanges",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "ShowroomRanges",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AvailabilityAr",
                table: "ShowroomProducts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CategoryAr",
                table: "ShowroomProducts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "ShowroomProducts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HairNeedAr",
                table: "ShowroomProducts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HairTypeAr",
                table: "ShowroomProducts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HowToUseAr",
                table: "ShowroomProducts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IngredientsAr",
                table: "ShowroomProducts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "ShowroomProducts",
                type: "character varying(240)",
                maxLength: 240,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShortDescriptionAr",
                table: "ShowroomProducts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelAr",
                table: "ShowroomNavigationItems",
                type: "character varying(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "ShowroomBrands",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "ShowroomBrands",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TaglineAr",
                table: "ShowroomBrands",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BodyAr",
                table: "ShowroomArticles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExcerptAr",
                table: "ShowroomArticles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleAr",
                table: "ShowroomArticles",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");
        }
    }
}
