using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Omranco.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddSomeProp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_Sku",
                table: "ProductVariants");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariantValues_ProductVariantId_ProductAttributeId",
                table: "ProductVariantValues",
                columns: new[] { "ProductVariantId", "ProductAttributeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_Sku",
                table: "ProductVariants",
                column: "Sku",
                unique: true,
                filter: "[Sku] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductVariantValues_ProductVariantId_ProductAttributeId",
                table: "ProductVariantValues");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_Sku",
                table: "ProductVariants");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_Sku",
                table: "ProductVariants",
                column: "Sku");
        }
    }
}
