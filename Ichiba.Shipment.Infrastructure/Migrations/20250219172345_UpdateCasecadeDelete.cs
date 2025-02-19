using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ichiba.Shipment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCasecadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentPackages_Packages_PackageId",
                table: "ShipmentPackages");

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentPackages_Packages_PackageId",
                table: "ShipmentPackages",
                column: "PackageId",
                principalTable: "Packages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentPackages_Packages_PackageId",
                table: "ShipmentPackages");

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentPackages_Packages_PackageId",
                table: "ShipmentPackages",
                column: "PackageId",
                principalTable: "Packages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
