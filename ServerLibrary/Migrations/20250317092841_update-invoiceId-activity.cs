using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServerLibrary.Migrations
{
    /// <inheritdoc />
    public partial class updateinvoiceIdactivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActivityId",
                table: "Invoices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvoiceId",
                table: "Activities",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ActivityId",
                table: "Invoices",
                column: "ActivityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Activities_ActivityId",
                table: "Invoices",
                column: "ActivityId",
                principalTable: "Activities",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Activities_ActivityId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_ActivityId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ActivityId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                table: "Activities");
        }
    }
}
