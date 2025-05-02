using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Projekt_NET.Migrations
{
    /// <inheritdoc />
    public partial class mig6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drones_DroneClouds_DroneCloudId",
                table: "Drones");

            migrationBuilder.AlterColumn<int>(
                name: "DroneCloudId",
                table: "Drones",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Drones_DroneClouds_DroneCloudId",
                table: "Drones",
                column: "DroneCloudId",
                principalTable: "DroneClouds",
                principalColumn: "DroneCloudId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drones_DroneClouds_DroneCloudId",
                table: "Drones");

            migrationBuilder.AlterColumn<int>(
                name: "DroneCloudId",
                table: "Drones",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Drones_DroneClouds_DroneCloudId",
                table: "Drones",
                column: "DroneCloudId",
                principalTable: "DroneClouds",
                principalColumn: "DroneCloudId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
