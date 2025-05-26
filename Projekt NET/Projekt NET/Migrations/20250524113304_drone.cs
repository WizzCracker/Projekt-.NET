using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Projekt_NET.Migrations
{
    /// <inheritdoc />
    public partial class drone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flights_Drones_DroneId",
                table: "Flights");

            migrationBuilder.AddForeignKey(
                name: "FK_Flights_Drones_DroneId",
                table: "Flights",
                column: "DroneId",
                principalTable: "Drones",
                principalColumn: "DroneId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flights_Drones_DroneId",
                table: "Flights");

            migrationBuilder.AddForeignKey(
                name: "FK_Flights_Drones_DroneId",
                table: "Flights",
                column: "DroneId",
                principalTable: "Drones",
                principalColumn: "DroneId");
        }
    }
}
