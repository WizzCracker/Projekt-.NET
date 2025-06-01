using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Projekt_NET.Migrations
{
    /// <inheritdoc />
    public partial class Last : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deliverylogs_Packages_PackageId",
                table: "Deliverylogs");

            migrationBuilder.DropForeignKey(
                name: "FK_DroneClouds_Operators_OperatorId",
                table: "DroneClouds");

            migrationBuilder.DropIndex(
                name: "IX_DroneClouds_OperatorId",
                table: "DroneClouds");

            migrationBuilder.DropIndex(
                name: "IX_Deliverylogs_PackageId",
                table: "Deliverylogs");

            migrationBuilder.DropColumn(
                name: "OperatorId",
                table: "DroneClouds");

            migrationBuilder.DropColumn(
                name: "PackageId",
                table: "Deliverylogs");

            migrationBuilder.RenameColumn(
                name: "DroneCloudId",
                table: "Operators",
                newName: "ClientId");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Deliverylogs",
                newName: "DeliveryId");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Deliveries",
                newName: "FlightPathId");

            migrationBuilder.AddColumn<int>(
                name: "DroneId",
                table: "Operators",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Deliverylogs_DeliveryId",
                table: "Deliverylogs",
                column: "DeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_FlightPathId",
                table: "Deliveries",
                column: "FlightPathId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_FlightPaths_FlightPathId",
                table: "Deliveries",
                column: "FlightPathId",
                principalTable: "FlightPaths",
                principalColumn: "FlightPathId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Deliverylogs_Deliveries_DeliveryId",
                table: "Deliverylogs",
                column: "DeliveryId",
                principalTable: "Deliveries",
                principalColumn: "DeliveryId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_FlightPaths_FlightPathId",
                table: "Deliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_Deliverylogs_Deliveries_DeliveryId",
                table: "Deliverylogs");

            migrationBuilder.DropIndex(
                name: "IX_Deliverylogs_DeliveryId",
                table: "Deliverylogs");

            migrationBuilder.DropIndex(
                name: "IX_Deliveries_FlightPathId",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "DroneId",
                table: "Operators");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "Operators",
                newName: "DroneCloudId");

            migrationBuilder.RenameColumn(
                name: "DeliveryId",
                table: "Deliverylogs",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "FlightPathId",
                table: "Deliveries",
                newName: "Type");

            migrationBuilder.AddColumn<int>(
                name: "OperatorId",
                table: "DroneClouds",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PackageId",
                table: "Deliverylogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DroneClouds_OperatorId",
                table: "DroneClouds",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Deliverylogs_PackageId",
                table: "Deliverylogs",
                column: "PackageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deliverylogs_Packages_PackageId",
                table: "Deliverylogs",
                column: "PackageId",
                principalTable: "Packages",
                principalColumn: "PackageId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DroneClouds_Operators_OperatorId",
                table: "DroneClouds",
                column: "OperatorId",
                principalTable: "Operators",
                principalColumn: "OperatorId");
        }
    }
}
