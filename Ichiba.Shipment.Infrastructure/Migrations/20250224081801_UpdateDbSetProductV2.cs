using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ichiba.Shipment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDbSetProductV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PackageProducts_Products_ProductId",
                table: "PackageProducts");

            migrationBuilder.DropTable(
                name: "Tax");

            migrationBuilder.AlterColumn<int>(
                name: "Unit",
                table: "PackageProducts",
                type: "integer",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "PackageProducts",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<decimal>(
                name: "Tax",
                table: "PackageProducts",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PackageProducts_Products_ProductId",
                table: "PackageProducts",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PackageProducts_Products_ProductId",
                table: "PackageProducts");

            migrationBuilder.DropColumn(
                name: "Tax",
                table: "PackageProducts");

            migrationBuilder.AlterColumn<double>(
                name: "Unit",
                table: "PackageProducts",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "PackageProducts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Tax",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    PackageProductId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tax", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tax_PackageProducts_PackageProductId",
                        column: x => x.PackageProductId,
                        principalTable: "PackageProducts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tax_PackageProductId",
                table: "Tax",
                column: "PackageProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_PackageProducts_Products_ProductId",
                table: "PackageProducts",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
