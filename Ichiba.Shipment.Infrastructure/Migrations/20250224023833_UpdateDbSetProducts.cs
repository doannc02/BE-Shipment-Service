using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ichiba.Shipment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDbSetProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SKU",
                table: "Products",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SKU",
                table: "Products");
        }
    }
}
