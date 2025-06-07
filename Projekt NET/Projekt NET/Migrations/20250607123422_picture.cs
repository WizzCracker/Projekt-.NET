using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Projekt_NET.Migrations
{
    /// <inheritdoc />
    public partial class picture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deliverylogs_Deliveries_DeliveryId",
                table: "Deliverylogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Deliverylogs",
                table: "Deliverylogs");

            migrationBuilder.RenameTable(
                name: "Deliverylogs",
                newName: "DeliveryLogs");

            migrationBuilder.RenameIndex(
                name: "IX_Deliverylogs_DeliveryId",
                table: "DeliveryLogs",
                newName: "IX_DeliveryLogs_DeliveryId");

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "Packages",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageMimeType",
                table: "Packages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Price",
                table: "Packages",
                type: "float",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeliveryLogs",
                table: "DeliveryLogs",
                column: "DeliveryLogId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryLogs_Deliveries_DeliveryId",
                table: "DeliveryLogs",
                column: "DeliveryId",
                principalTable: "Deliveries",
                principalColumn: "DeliveryId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryLogs_Deliveries_DeliveryId",
                table: "DeliveryLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeliveryLogs",
                table: "DeliveryLogs");

            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "ImageMimeType",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Packages");

            migrationBuilder.RenameTable(
                name: "DeliveryLogs",
                newName: "Deliverylogs");

            migrationBuilder.RenameIndex(
                name: "IX_DeliveryLogs_DeliveryId",
                table: "Deliverylogs",
                newName: "IX_Deliverylogs_DeliveryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Deliverylogs",
                table: "Deliverylogs",
                column: "DeliveryLogId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deliverylogs_Deliveries_DeliveryId",
                table: "Deliverylogs",
                column: "DeliveryId",
                principalTable: "Deliveries",
                principalColumn: "DeliveryId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
