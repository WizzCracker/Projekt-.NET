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
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Districts_DistrictId",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_DistrictId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "Clients");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DistrictId",
                table: "Clients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_DistrictId",
                table: "Clients",
                column: "DistrictId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Districts_DistrictId",
                table: "Clients",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "DistrictId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
