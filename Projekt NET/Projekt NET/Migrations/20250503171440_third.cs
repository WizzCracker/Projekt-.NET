using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Projekt_NET.Migrations
{
    /// <inheritdoc />
    public partial class third : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Flights_DeliveryCoordinates",
                table: "Flights_DeliveryCoordinates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Districts_BoundingPoints",
                table: "Districts_BoundingPoints");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "DroneClouds",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Flights_DeliveryCoordinates",
                table: "Flights_DeliveryCoordinates",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Districts_BoundingPoints",
                table: "Districts_BoundingPoints",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_DeliveryCoordinates_FlightId",
                table: "Flights_DeliveryCoordinates",
                column: "FlightId");

            migrationBuilder.CreateIndex(
                name: "IX_Districts_BoundingPoints_DistrictId",
                table: "Districts_BoundingPoints",
                column: "DistrictId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Flights_DeliveryCoordinates",
                table: "Flights_DeliveryCoordinates");

            migrationBuilder.DropIndex(
                name: "IX_Flights_DeliveryCoordinates_FlightId",
                table: "Flights_DeliveryCoordinates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Districts_BoundingPoints",
                table: "Districts_BoundingPoints");

            migrationBuilder.DropIndex(
                name: "IX_Districts_BoundingPoints_DistrictId",
                table: "Districts_BoundingPoints");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "DroneClouds");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Flights_DeliveryCoordinates",
                table: "Flights_DeliveryCoordinates",
                columns: new[] { "FlightId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Districts_BoundingPoints",
                table: "Districts_BoundingPoints",
                columns: new[] { "DistrictId", "Id" });
        }
    }
}
