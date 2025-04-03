using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServerLibrary.Migrations
{
    /// <inheritdoc />
    public partial class updateinventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "OrderQuantity",
                table: "ProductInventories",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "MinimumStockLevel",
                table: "ProductInventories",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "AvailableQuantity",
                table: "ProductInventories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "ProductInventories",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "ReturnedQuantity",
                table: "ProductInventories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierName",
                table: "ProductInventories",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableQuantity",
                table: "ProductInventories");

            migrationBuilder.DropColumn(
                name: "Brand",
                table: "ProductInventories");

            migrationBuilder.DropColumn(
                name: "ReturnedQuantity",
                table: "ProductInventories");

            migrationBuilder.DropColumn(
                name: "SupplierName",
                table: "ProductInventories");

            migrationBuilder.AlterColumn<int>(
                name: "OrderQuantity",
                table: "ProductInventories",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MinimumStockLevel",
                table: "ProductInventories",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
