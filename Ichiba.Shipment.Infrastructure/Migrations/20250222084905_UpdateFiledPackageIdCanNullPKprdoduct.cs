using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ichiba.Shipment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFiledPackageIdCanNullPKprdoduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PackageProducts_Packages_PackageId",
                table: "PackageProducts");

            migrationBuilder.AlterColumn<Guid>(
                name: "PackageId",
                table: "PackageProducts",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_PackageProducts_Packages_PackageId",
                table: "PackageProducts",
                column: "PackageId",
                principalTable: "Packages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PackageProducts_Packages_PackageId",
                table: "PackageProducts");

            migrationBuilder.AlterColumn<Guid>(
                name: "PackageId",
                table: "PackageProducts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PackageProducts_Packages_PackageId",
                table: "PackageProducts",
                column: "PackageId",
                principalTable: "Packages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
