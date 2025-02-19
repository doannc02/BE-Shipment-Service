using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ichiba.Shipment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePackageStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"Shipments\" ALTER COLUMN \"Status\" TYPE integer USING \"Status\"::integer;");
            migrationBuilder.Sql("ALTER TABLE \"Packages\" ALTER COLUMN \"Status\" TYPE integer USING \"Status\"::integer;");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Shipments",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Packages",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "Packages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ShipmentAddressId",
                table: "Packages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Packages_ShipmentAddressId",
                table: "Packages",
                column: "ShipmentAddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_Packages_ShipmentAddresses_ShipmentAddressId",
                table: "Packages",
                column: "ShipmentAddressId",
                principalTable: "ShipmentAddresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Packages_ShipmentAddresses_ShipmentAddressId",
                table: "Packages");

            migrationBuilder.DropIndex(
                name: "IX_Packages_ShipmentAddressId",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "ShipmentAddressId",
                table: "Packages");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Shipments",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Packages",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
