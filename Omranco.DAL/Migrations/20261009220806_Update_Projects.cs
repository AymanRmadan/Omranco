using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Omranco.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Update_Projects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ZoneId",
                table: "Projects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ZoneId",
                table: "Projects",
                column: "ZoneId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Zones_ZoneId",
                table: "Projects",
                column: "ZoneId",
                principalTable: "Zones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Zones_ZoneId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_ZoneId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ZoneId",
                table: "Projects");
        }
    }
}
