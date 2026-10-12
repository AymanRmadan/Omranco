using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Omranco.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorRatingss : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VendorRatings_CustomerAccountUsers_CustomerAccountUserId",
                table: "VendorRatings");

            migrationBuilder.DropIndex(
                name: "IX_VendorRatings_VendorId",
                table: "VendorRatings");

            migrationBuilder.RenameColumn(
                name: "CustomerAccountUserId",
                table: "VendorRatings",
                newName: "RatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_VendorRatings_CustomerAccountUserId",
                table: "VendorRatings",
                newName: "IX_VendorRatings_RatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorRatings_VendorId_CreatedAt",
                table: "VendorRatings",
                columns: new[] { "VendorId", "CreatedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_VendorRatings_AspNetUsers_RatedByUserId",
                table: "VendorRatings",
                column: "RatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VendorRatings_AspNetUsers_RatedByUserId",
                table: "VendorRatings");

            migrationBuilder.DropIndex(
                name: "IX_VendorRatings_VendorId_CreatedAt",
                table: "VendorRatings");

            migrationBuilder.RenameColumn(
                name: "RatedByUserId",
                table: "VendorRatings",
                newName: "CustomerAccountUserId");

            migrationBuilder.RenameIndex(
                name: "IX_VendorRatings_RatedByUserId",
                table: "VendorRatings",
                newName: "IX_VendorRatings_CustomerAccountUserId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorRatings_VendorId",
                table: "VendorRatings",
                column: "VendorId");

            migrationBuilder.AddForeignKey(
                name: "FK_VendorRatings_CustomerAccountUsers_CustomerAccountUserId",
                table: "VendorRatings",
                column: "CustomerAccountUserId",
                principalTable: "CustomerAccountUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
