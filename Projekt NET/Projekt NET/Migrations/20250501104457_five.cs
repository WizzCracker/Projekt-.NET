using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Projekt_NET.Migrations
{
    /// <inheritdoc />
    public partial class five : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "status",
                table: "Drones",
                newName: "Status");

            migrationBuilder.AddColumn<int>(
                name: "OperatorId",
                table: "DroneClouds",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Delivery",
                columns: table => new
                {
                    DeliveryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    PackageId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Delivery", x => x.DeliveryId);
                    table.ForeignKey(
                        name: "FK_Delivery_Packages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "Packages",
                        principalColumn: "PackageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Deliverylogs",
                columns: table => new
                {
                    DeliveryLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackageId = table.Column<int>(type: "int", nullable: false),
                    LogDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deliverylogs", x => x.DeliveryLogId);
                    table.ForeignKey(
                        name: "FK_Deliverylogs_Packages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "Packages",
                        principalColumn: "PackageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FlightPaths",
                columns: table => new
                {
                    FlightPathId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightPaths", x => x.FlightPathId);
                });

            migrationBuilder.CreateTable(
                name: "Operators",
                columns: table => new
                {
                    OperatorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operators", x => x.OperatorId);
                });

            migrationBuilder.CreateTable(
                name: "Flights",
                columns: table => new
                {
                    FlightId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ArrivDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryCoordinates = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FlightPathId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flights", x => x.FlightId);
                    table.ForeignKey(
                        name: "FK_Flights_FlightPaths_FlightPathId",
                        column: x => x.FlightPathId,
                        principalTable: "FlightPaths",
                        principalColumn: "FlightPathId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DroneClouds_OperatorId",
                table: "DroneClouds",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Delivery_PackageId",
                table: "Delivery",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_Deliverylogs_PackageId",
                table: "Deliverylogs",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_FlightPathId",
                table: "Flights",
                column: "FlightPathId");

            migrationBuilder.AddForeignKey(
                name: "FK_DroneClouds_Operators_OperatorId",
                table: "DroneClouds",
                column: "OperatorId",
                principalTable: "Operators",
                principalColumn: "OperatorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DroneClouds_Operators_OperatorId",
                table: "DroneClouds");

            migrationBuilder.DropTable(
                name: "Delivery");

            migrationBuilder.DropTable(
                name: "Deliverylogs");

            migrationBuilder.DropTable(
                name: "Flights");

            migrationBuilder.DropTable(
                name: "Operators");

            migrationBuilder.DropTable(
                name: "FlightPaths");

            migrationBuilder.DropIndex(
                name: "IX_DroneClouds_OperatorId",
                table: "DroneClouds");

            migrationBuilder.DropColumn(
                name: "OperatorId",
                table: "DroneClouds");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Drones",
                newName: "status");
        }
    }
}
