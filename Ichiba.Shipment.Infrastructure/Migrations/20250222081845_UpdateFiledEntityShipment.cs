using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ichiba.Shipment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFiledEntityShipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CubitUnit",
                table: "Shipments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WeightUnit",
                table: "Shipments",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CubitUnit",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "WeightUnit",
                table: "Shipments");
        }
    }
}
