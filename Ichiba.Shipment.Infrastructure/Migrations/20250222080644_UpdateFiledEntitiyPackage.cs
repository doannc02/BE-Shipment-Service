using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ichiba.Shipment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFiledEntitiyPackage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CubitUnit",
                table: "Packages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WeightUnit",
                table: "Packages",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CubitUnit",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "WeightUnit",
                table: "Packages");
        }
    }
}
