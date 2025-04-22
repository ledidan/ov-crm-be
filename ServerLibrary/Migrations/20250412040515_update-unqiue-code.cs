using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServerLibrary.Migrations
{
    /// <inheritdoc />
    public partial class updateunqiuecode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerCares_Partners_PartnerId",
                table: "CustomerCares");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Partners_PartnerId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Partners_PartnerId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductCategories_Partners_PartnerId",
                table: "ProductCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductInventories_Partners_PartnerId",
                table: "ProductInventories");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Partners_PartnerId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportTickets_Partners_PartnerId",
                table: "SupportTickets");

            migrationBuilder.AlterColumn<string>(
                name: "TicketNumber",
                table: "SupportTickets",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "PartnerId",
                table: "SupportTickets",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PartnerId",
                table: "Products",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PartnerId",
                table: "ProductInventories",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "ProductCategoryCode",
                table: "ProductCategories",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "PartnerId",
                table: "ProductCategories",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "SaleOrderNo",
                table: "Orders",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "PartnerId",
                table: "Orders",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PartnerId",
                table: "Invoices",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceRequestName",
                table: "Invoices",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "AccountNumber",
                table: "Customers",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "PartnerId",
                table: "CustomerCares",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerCareNumber",
                table: "CustomerCares",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ContactCode",
                table: "Contacts",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_TicketNumber_PartnerId",
                table: "SupportTickets",
                columns: new[] { "TicketNumber", "PartnerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductCode_PartnerId",
                table: "Products",
                columns: new[] { "ProductCode", "PartnerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_ProductCategoryCode_PartnerId",
                table: "ProductCategories",
                columns: new[] { "ProductCategoryCode", "PartnerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SaleOrderNo_PartnerId",
                table: "Orders",
                columns: new[] { "SaleOrderNo", "PartnerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceRequestName_PartnerId",
                table: "Invoices",
                columns: new[] { "InvoiceRequestName", "PartnerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_AccountNumber_PartnerId",
                table: "Customers",
                columns: new[] { "AccountNumber", "PartnerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerCares_CustomerCareNumber_PartnerId",
                table: "CustomerCares",
                columns: new[] { "CustomerCareNumber", "PartnerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_ContactCode_PartnerId",
                table: "Contacts",
                columns: new[] { "ContactCode", "PartnerId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerCares_Partners_PartnerId",
                table: "CustomerCares",
                column: "PartnerId",
                principalTable: "Partners",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Partners_PartnerId",
                table: "Invoices",
                column: "PartnerId",
                principalTable: "Partners",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Partners_PartnerId",
                table: "Orders",
                column: "PartnerId",
                principalTable: "Partners",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCategories_Partners_PartnerId",
                table: "ProductCategories",
                column: "PartnerId",
                principalTable: "Partners",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductInventories_Partners_PartnerId",
                table: "ProductInventories",
                column: "PartnerId",
                principalTable: "Partners",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Partners_PartnerId",
                table: "Products",
                column: "PartnerId",
                principalTable: "Partners",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTickets_Partners_PartnerId",
                table: "SupportTickets",
                column: "PartnerId",
                principalTable: "Partners",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerCares_Partners_PartnerId",
                table: "CustomerCares");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Partners_PartnerId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Partners_PartnerId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductCategories_Partners_PartnerId",
                table: "ProductCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductInventories_Partners_PartnerId",
                table: "ProductInventories");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Partners_PartnerId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportTickets_Partners_PartnerId",
                table: "SupportTickets");

            migrationBuilder.DropIndex(
                name: "IX_SupportTickets_TicketNumber_PartnerId",
                table: "SupportTickets");

            migrationBuilder.DropIndex(
                name: "IX_Products_ProductCode_PartnerId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_ProductCategories_ProductCategoryCode_PartnerId",
                table: "ProductCategories");

            migrationBuilder.DropIndex(
                name: "IX_Orders_SaleOrderNo_PartnerId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_InvoiceRequestName_PartnerId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Customers_AccountNumber_PartnerId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_CustomerCares_CustomerCareNumber_PartnerId",
                table: "CustomerCares");

            migrationBuilder.DropIndex(
                name: "IX_Contacts_ContactCode_PartnerId",
                table: "Contacts");

            migrationBuilder.AlterColumn<string>(
                name: "TicketNumber",
                table: "SupportTickets",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "PartnerId",
                table: "SupportTickets",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PartnerId",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PartnerId",
                table: "ProductInventories",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProductCategoryCode",
                table: "ProductCategories",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "PartnerId",
                table: "ProductCategories",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SaleOrderNo",
                table: "Orders",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "PartnerId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PartnerId",
                table: "Invoices",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceRequestName",
                table: "Invoices",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "AccountNumber",
                table: "Customers",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "PartnerId",
                table: "CustomerCares",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerCareNumber",
                table: "CustomerCares",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ContactCode",
                table: "Contacts",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerCares_Partners_PartnerId",
                table: "CustomerCares",
                column: "PartnerId",
                principalTable: "Partners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Partners_PartnerId",
                table: "Invoices",
                column: "PartnerId",
                principalTable: "Partners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Partners_PartnerId",
                table: "Orders",
                column: "PartnerId",
                principalTable: "Partners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCategories_Partners_PartnerId",
                table: "ProductCategories",
                column: "PartnerId",
                principalTable: "Partners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductInventories_Partners_PartnerId",
                table: "ProductInventories",
                column: "PartnerId",
                principalTable: "Partners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Partners_PartnerId",
                table: "Products",
                column: "PartnerId",
                principalTable: "Partners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTickets_Partners_PartnerId",
                table: "SupportTickets",
                column: "PartnerId",
                principalTable: "Partners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
