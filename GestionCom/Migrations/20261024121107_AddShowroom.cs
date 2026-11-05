using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GestionCom.Migrations
{
    /// <inheritdoc />
    public partial class AddShowroom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShowroomArticles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Slug = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    TitleFr = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    TitleAr = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ExcerptFr = table.Column<string>(type: "text", nullable: true),
                    ExcerptAr = table.Column<string>(type: "text", nullable: true),
                    BodyFr = table.Column<string>(type: "text", nullable: true),
                    BodyAr = table.Column<string>(type: "text", nullable: true),
                    CoverImagePath = table.Column<string>(type: "text", nullable: true),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShowroomArticles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShowroomBrands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    NameFr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TaglineFr = table.Column<string>(type: "text", nullable: true),
                    TaglineAr = table.Column<string>(type: "text", nullable: true),
                    DescriptionFr = table.Column<string>(type: "text", nullable: true),
                    DescriptionAr = table.Column<string>(type: "text", nullable: true),
                    LogoImagePath = table.Column<string>(type: "text", nullable: true),
                    HeroImagePath = table.Column<string>(type: "text", nullable: true),
                    AccentColor = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShowroomBrands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShowroomNavigationItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    LabelFr = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    LabelAr = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false),
                    OpenInNewTab = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShowroomNavigationItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShowroomPageSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PageKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SectionKey = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    SectionType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    TitleFr = table.Column<string>(type: "text", nullable: true),
                    TitleAr = table.Column<string>(type: "text", nullable: true),
                    SubtitleFr = table.Column<string>(type: "text", nullable: true),
                    SubtitleAr = table.Column<string>(type: "text", nullable: true),
                    BodyFr = table.Column<string>(type: "text", nullable: true),
                    BodyAr = table.Column<string>(type: "text", nullable: true),
                    ImagePath = table.Column<string>(type: "text", nullable: true),
                    MobileImagePath = table.Column<string>(type: "text", nullable: true),
                    ButtonLabelFr = table.Column<string>(type: "text", nullable: true),
                    ButtonLabelAr = table.Column<string>(type: "text", nullable: true),
                    ButtonUrl = table.Column<string>(type: "text", nullable: true),
                    SecondaryButtonLabelFr = table.Column<string>(type: "text", nullable: true),
                    SecondaryButtonLabelAr = table.Column<string>(type: "text", nullable: true),
                    SecondaryButtonUrl = table.Column<string>(type: "text", nullable: true),
                    StyleVariant = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShowroomPageSections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShowroomRoutines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Slug = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    TitleFr = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    TitleAr = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    DescriptionFr = table.Column<string>(type: "text", nullable: true),
                    DescriptionAr = table.Column<string>(type: "text", nullable: true),
                    HairNeedFr = table.Column<string>(type: "text", nullable: true),
                    HairNeedAr = table.Column<string>(type: "text", nullable: true),
                    ImagePath = table.Column<string>(type: "text", nullable: true),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShowroomRoutines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShowroomServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Slug = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    TitleFr = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    TitleAr = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    ShortDescriptionFr = table.Column<string>(type: "text", nullable: true),
                    ShortDescriptionAr = table.Column<string>(type: "text", nullable: true),
                    DescriptionFr = table.Column<string>(type: "text", nullable: true),
                    DescriptionAr = table.Column<string>(type: "text", nullable: true),
                    ImagePath = table.Column<string>(type: "text", nullable: true),
                    ButtonLabelFr = table.Column<string>(type: "text", nullable: true),
                    ButtonLabelAr = table.Column<string>(type: "text", nullable: true),
                    ButtonUrl = table.Column<string>(type: "text", nullable: true),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShowroomServices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShowroomSiteSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    ValueFr = table.Column<string>(type: "text", nullable: true),
                    ValueAr = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShowroomSiteSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShowroomRanges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BrandId = table.Column<int>(type: "integer", nullable: false),
                    Slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    NameFr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DescriptionFr = table.Column<string>(type: "text", nullable: true),
                    DescriptionAr = table.Column<string>(type: "text", nullable: true),
                    ImagePath = table.Column<string>(type: "text", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShowroomRanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShowroomRanges_ShowroomBrands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "ShowroomBrands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShowroomProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BrandId = table.Column<int>(type: "integer", nullable: false),
                    RangeId = table.Column<int>(type: "integer", nullable: true),
                    Slug = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    NameFr = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    CategoryFr = table.Column<string>(type: "text", nullable: true),
                    CategoryAr = table.Column<string>(type: "text", nullable: true),
                    HairNeedFr = table.Column<string>(type: "text", nullable: true),
                    HairNeedAr = table.Column<string>(type: "text", nullable: true),
                    ShortDescriptionFr = table.Column<string>(type: "text", nullable: true),
                    ShortDescriptionAr = table.Column<string>(type: "text", nullable: true),
                    DescriptionFr = table.Column<string>(type: "text", nullable: true),
                    DescriptionAr = table.Column<string>(type: "text", nullable: true),
                    HairTypeFr = table.Column<string>(type: "text", nullable: true),
                    HairTypeAr = table.Column<string>(type: "text", nullable: true),
                    HowToUseFr = table.Column<string>(type: "text", nullable: true),
                    HowToUseAr = table.Column<string>(type: "text", nullable: true),
                    IngredientsFr = table.Column<string>(type: "text", nullable: true),
                    IngredientsAr = table.Column<string>(type: "text", nullable: true),
                    Size = table.Column<string>(type: "text", nullable: true),
                    PriceTnd = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AvailabilityFr = table.Column<string>(type: "text", nullable: true),
                    AvailabilityAr = table.Column<string>(type: "text", nullable: true),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                    IsNew = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShowroomProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShowroomProducts_ShowroomBrands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "ShowroomBrands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShowroomProducts_ShowroomRanges_RangeId",
                        column: x => x.RangeId,
                        principalTable: "ShowroomRanges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ShowroomProductImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    ImagePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AltTextFr = table.Column<string>(type: "text", nullable: true),
                    AltTextAr = table.Column<string>(type: "text", nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShowroomProductImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShowroomProductImages_ShowroomProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "ShowroomProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShowroomRoutineProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoutineId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    StepLabelFr = table.Column<string>(type: "text", nullable: true),
                    StepLabelAr = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShowroomRoutineProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShowroomRoutineProducts_ShowroomProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "ShowroomProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShowroomRoutineProducts_ShowroomRoutines_RoutineId",
                        column: x => x.RoutineId,
                        principalTable: "ShowroomRoutines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShowroomArticles_Slug",
                table: "ShowroomArticles",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShowroomBrands_Slug",
                table: "ShowroomBrands",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShowroomNavigationItems_Key",
                table: "ShowroomNavigationItems",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShowroomPageSections_PageKey_SectionKey",
                table: "ShowroomPageSections",
                columns: new[] { "PageKey", "SectionKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShowroomProductImages_ProductId_SortOrder",
                table: "ShowroomProductImages",
                columns: new[] { "ProductId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ShowroomProducts_BrandId",
                table: "ShowroomProducts",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_ShowroomProducts_IsPublished_SortOrder",
                table: "ShowroomProducts",
                columns: new[] { "IsPublished", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ShowroomProducts_RangeId",
                table: "ShowroomProducts",
                column: "RangeId");

            migrationBuilder.CreateIndex(
                name: "IX_ShowroomProducts_Slug",
                table: "ShowroomProducts",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShowroomRanges_BrandId_Slug",
                table: "ShowroomRanges",
                columns: new[] { "BrandId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShowroomRoutineProducts_ProductId",
                table: "ShowroomRoutineProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ShowroomRoutineProducts_RoutineId_ProductId",
                table: "ShowroomRoutineProducts",
                columns: new[] { "RoutineId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShowroomRoutines_Slug",
                table: "ShowroomRoutines",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShowroomServices_Slug",
                table: "ShowroomServices",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShowroomSiteSettings_Key",
                table: "ShowroomSiteSettings",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShowroomArticles");

            migrationBuilder.DropTable(
                name: "ShowroomNavigationItems");

            migrationBuilder.DropTable(
                name: "ShowroomPageSections");

            migrationBuilder.DropTable(
                name: "ShowroomProductImages");

            migrationBuilder.DropTable(
                name: "ShowroomRoutineProducts");

            migrationBuilder.DropTable(
                name: "ShowroomServices");

            migrationBuilder.DropTable(
                name: "ShowroomSiteSettings");

            migrationBuilder.DropTable(
                name: "ShowroomProducts");

            migrationBuilder.DropTable(
                name: "ShowroomRoutines");

            migrationBuilder.DropTable(
                name: "ShowroomRanges");

            migrationBuilder.DropTable(
                name: "ShowroomBrands");
        }
    }
}
