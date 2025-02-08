using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServerLibrary.Migrations
{
    /// <inheritdoc />
    public partial class fixproductcategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductCategories_ProductCategories_ParentProductCategoryID",
                table: "ProductCategories");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCategories_ProductCategories_ParentProductCategoryID",
                table: "ProductCategories",
                column: "ParentProductCategoryID",
                principalTable: "ProductCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductCategories_ProductCategories_ParentProductCategoryID",
                table: "ProductCategories");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCategories_ProductCategories_ParentProductCategoryID",
                table: "ProductCategories",
                column: "ParentProductCategoryID",
                principalTable: "ProductCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
