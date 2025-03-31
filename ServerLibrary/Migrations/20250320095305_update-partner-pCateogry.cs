using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServerLibrary.Migrations
{
    /// <inheritdoc />
    public partial class updatepartnerpCateogry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PartnerId",
                table: "ProductInventories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ProductInventories_PartnerId",
                table: "ProductInventories",
                column: "PartnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductInventories_Partners_PartnerId",
                table: "ProductInventories",
                column: "PartnerId",
                principalTable: "Partners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductInventories_Partners_PartnerId",
                table: "ProductInventories");

            migrationBuilder.DropIndex(
                name: "IX_ProductInventories_PartnerId",
                table: "ProductInventories");

            migrationBuilder.DropColumn(
                name: "PartnerId",
                table: "ProductInventories");
        }
    }
}
