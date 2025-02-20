using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ichiba.Shipment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFieldInShipmentAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CarrierId",
                table: "Shipments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "ShipmentAddresses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "ShipmentAddresses",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "ShipmentAddresses",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CarrierId",
                table: "Packages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CarrierId",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "ShipmentAddresses");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "ShipmentAddresses");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "ShipmentAddresses");

            migrationBuilder.DropColumn(
                name: "CarrierId",
                table: "Packages");
        }
    }
}
