using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ichiba.Shipment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDbSetProductV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PackageProducts_ProductId",
                table: "PackageProducts",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_PackageProducts_Products_ProductId",
                table: "PackageProducts",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PackageProducts_Products_ProductId",
                table: "PackageProducts");

            migrationBuilder.DropIndex(
                name: "IX_PackageProducts_ProductId",
                table: "PackageProducts");
        }
    }
}
