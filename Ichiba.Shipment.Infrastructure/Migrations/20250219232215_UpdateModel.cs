using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ichiba.Shipment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PackageAddress_Packages_PackageId",
                table: "PackageAddress");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PackageAddress",
                table: "PackageAddress");

            migrationBuilder.RenameTable(
                name: "PackageAddress",
                newName: "PackageAddresses");

            migrationBuilder.RenameIndex(
                name: "IX_PackageAddress_PackageId",
                table: "PackageAddresses",
                newName: "IX_PackageAddresses_PackageId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PackageAddresses",
                table: "PackageAddresses",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PackageAddresses_Packages_PackageId",
                table: "PackageAddresses",
                column: "PackageId",
                principalTable: "Packages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PackageAddresses_Packages_PackageId",
                table: "PackageAddresses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PackageAddresses",
                table: "PackageAddresses");

            migrationBuilder.RenameTable(
                name: "PackageAddresses",
                newName: "PackageAddress");

            migrationBuilder.RenameIndex(
                name: "IX_PackageAddresses_PackageId",
                table: "PackageAddress",
                newName: "IX_PackageAddress_PackageId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PackageAddress",
                table: "PackageAddress",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PackageAddress_Packages_PackageId",
                table: "PackageAddress",
                column: "PackageId",
                principalTable: "Packages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
