using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Projekt_NET.Migrations
{
    /// <inheritdoc />
    public partial class drone2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Packages_Drones_DroneId",
                table: "Packages");

            migrationBuilder.AddForeignKey(
                name: "FK_Packages_Drones_DroneId",
                table: "Packages",
                column: "DroneId",
                principalTable: "Drones",
                principalColumn: "DroneId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Packages_Drones_DroneId",
                table: "Packages");

            migrationBuilder.AddForeignKey(
                name: "FK_Packages_Drones_DroneId",
                table: "Packages",
                column: "DroneId",
                principalTable: "Drones",
                principalColumn: "DroneId");
        }
    }
}
