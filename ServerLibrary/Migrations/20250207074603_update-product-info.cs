using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServerLibrary.Migrations
{
    /// <inheritdoc />
    public partial class updateproductinfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "Unique_Code_PartnerId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "WarrantyPeriodPerMonth",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "Unit",
                table: "Products",
                newName: "WarrantyPeriodTypeID");

            migrationBuilder.RenameColumn(
                name: "ProducerName",
                table: "Products",
                newName: "WarrantyPeriod");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Products",
                newName: "WarrantyDescription");

            migrationBuilder.AddColumn<decimal>(
                name: "AmountSummary",
                table: "Products",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Avatar",
                table: "Products",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "ConversionRate",
                table: "Products",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConversionUnit",
                table: "Products",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Products",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CustomID",
                table: "Products",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Products",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Equation",
                table: "Products",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "Inactive",
                table: "Products",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InventoryItemID",
                table: "Products",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Products",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFollowSerialNumber",
                table: "Products",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Products",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSetProduct",
                table: "Products",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSystem",
                table: "Products",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsUseTax",
                table: "Products",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Products",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "OldProductCode",
                table: "Products",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "OperatorID",
                table: "Products",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "OwnerID",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PriceAfterTax",
                table: "Products",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ProductCode",
                table: "Products",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ProductGroupID",
                table: "Products",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ProductGroupName",
                table: "Products",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "Products",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ProductPropertiesID",
                table: "Products",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "PurchasedPrice",
                table: "Products",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "QuantityDemanded",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuantityFormula",
                table: "Products",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "QuantityInstock",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuantityOrdered",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SaleDescription",
                table: "Products",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SearchTagID",
                table: "Products",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TagColor",
                table: "Products",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TagID",
                table: "Products",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TaxID",
                table: "Products",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "Taxable",
                table: "Products",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitCost",
                table: "Products",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "Products",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice1",
                table: "Products",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice2",
                table: "Products",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPriceFixed",
                table: "Products",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "UsageUnitID",
                table: "Products",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "VendorNameID",
                table: "Products",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "Unique_ProductCode_PartnerId",
                table: "Products",
                columns: new[] { "ProductCode", "PartnerId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "Unique_ProductCode_PartnerId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "AmountSummary",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Avatar",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ConversionRate",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ConversionUnit",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CustomID",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Equation",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Inactive",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "InventoryItemID",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsFollowSerialNumber",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsSetProduct",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsSystem",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsUseTax",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "OldProductCode",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "OperatorID",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "OwnerID",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PriceAfterTax",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductCode",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductGroupID",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductGroupName",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductPropertiesID",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PurchasedPrice",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "QuantityDemanded",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "QuantityFormula",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "QuantityInstock",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "QuantityOrdered",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SaleDescription",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SearchTagID",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TagColor",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TagID",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TaxID",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Taxable",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UnitCost",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UnitPrice1",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UnitPrice2",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UnitPriceFixed",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UsageUnitID",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "VendorNameID",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "WarrantyPeriodTypeID",
                table: "Products",
                newName: "Unit");

            migrationBuilder.RenameColumn(
                name: "WarrantyPeriod",
                table: "Products",
                newName: "ProducerName");

            migrationBuilder.RenameColumn(
                name: "WarrantyDescription",
                table: "Products",
                newName: "Name");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Products",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "WarrantyPeriodPerMonth",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "Unique_Code_PartnerId",
                table: "Products",
                columns: new[] { "Code", "PartnerId" },
                unique: true);
        }
    }
}
