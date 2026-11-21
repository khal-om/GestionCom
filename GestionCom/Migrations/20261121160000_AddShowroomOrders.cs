using GestionCom.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable
namespace GestionCom.Migrations;
[DbContext(typeof(AppDbContext))]
[Migration("20261121160000_AddShowroomOrders")]
public partial class AddShowroomOrders : Migration
{
 protected override void Up(MigrationBuilder migrationBuilder)
 {
  migrationBuilder.CreateTable(name:"ShowroomOrders", columns:table=>new
  {
   Id=table.Column<int>(type:"integer",nullable:false).Annotation("Npgsql:ValueGenerationStrategy",NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
   OrderNumber=table.Column<string>(type:"character varying(32)",maxLength:32,nullable:false),
   CustomerName=table.Column<string>(type:"character varying(180)",maxLength:180,nullable:false),
   Email=table.Column<string>(type:"character varying(254)",maxLength:254,nullable:false),
   Phone=table.Column<string>(type:"character varying(40)",maxLength:40,nullable:false),
   Address=table.Column<string>(type:"character varying(240)",maxLength:240,nullable:false),
   PostalCode=table.Column<string>(type:"character varying(32)",maxLength:32,nullable:false),
   City=table.Column<string>(type:"character varying(120)",maxLength:120,nullable:false),
   Country=table.Column<string>(type:"character varying(120)",maxLength:120,nullable:false),
   Status=table.Column<string>(type:"character varying(40)",maxLength:40,nullable:false),
   AdminNote=table.Column<string>(type:"text",nullable:true),
   TotalTnd=table.Column<decimal>(type:"numeric(18,2)",precision:18,scale:2,nullable:false),
   CreatedAt=table.Column<DateTime>(type:"timestamp with time zone",nullable:false),
   CreatedBy=table.Column<int>(type:"integer",nullable:true),
   UpdatedAt=table.Column<DateTime>(type:"timestamp with time zone",nullable:true),
   UpdatedBy=table.Column<int>(type:"integer",nullable:true)
  },constraints:table=>table.PrimaryKey("PK_ShowroomOrders",x=>x.Id));
  migrationBuilder.CreateTable(name:"ShowroomOrderItems",columns:table=>new
  {
   Id=table.Column<int>(type:"integer",nullable:false).Annotation("Npgsql:ValueGenerationStrategy",NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
   ShowroomOrderId=table.Column<int>(type:"integer",nullable:false),
   ShowroomProductId=table.Column<int>(type:"integer",nullable:true),
   ProductNameFr=table.Column<string>(type:"character varying(240)",maxLength:240,nullable:false),
   BrandNameFr=table.Column<string>(type:"character varying(200)",maxLength:200,nullable:false),
   Size=table.Column<string>(type:"character varying(120)",maxLength:120,nullable:true),
   UnitPriceTnd=table.Column<decimal>(type:"numeric(18,2)",precision:18,scale:2,nullable:false),
   Quantity=table.Column<int>(type:"integer",nullable:false),
   CreatedAt=table.Column<DateTime>(type:"timestamp with time zone",nullable:false),
   CreatedBy=table.Column<int>(type:"integer",nullable:true),
   UpdatedAt=table.Column<DateTime>(type:"timestamp with time zone",nullable:true),
   UpdatedBy=table.Column<int>(type:"integer",nullable:true)
  },constraints:table=>
  {
   table.PrimaryKey("PK_ShowroomOrderItems",x=>x.Id);
   table.ForeignKey(name:"FK_ShowroomOrderItems_ShowroomOrders_ShowroomOrderId",column:x=>x.ShowroomOrderId,principalTable:"ShowroomOrders",principalColumn:"Id",onDelete:ReferentialAction.Cascade);
   table.ForeignKey(name:"FK_ShowroomOrderItems_ShowroomProducts_ShowroomProductId",column:x=>x.ShowroomProductId,principalTable:"ShowroomProducts",principalColumn:"Id",onDelete:ReferentialAction.SetNull);
  });
  migrationBuilder.CreateIndex(name:"IX_ShowroomOrders_OrderNumber",table:"ShowroomOrders",column:"OrderNumber",unique:true);
  migrationBuilder.CreateIndex(name:"IX_ShowroomOrders_CreatedAt",table:"ShowroomOrders",column:"CreatedAt");
  migrationBuilder.CreateIndex(name:"IX_ShowroomOrderItems_ShowroomOrderId",table:"ShowroomOrderItems",column:"ShowroomOrderId");
  migrationBuilder.CreateIndex(name:"IX_ShowroomOrderItems_ShowroomProductId",table:"ShowroomOrderItems",column:"ShowroomProductId");
 }
 protected override void Down(MigrationBuilder migrationBuilder)
 {
  migrationBuilder.DropTable(name:"ShowroomOrderItems");
  migrationBuilder.DropTable(name:"ShowroomOrders");
 }
}