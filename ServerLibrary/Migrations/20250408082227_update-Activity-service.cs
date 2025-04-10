using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServerLibrary.Migrations
{
    /// <inheritdoc />
    public partial class updateActivityservice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CustomerCareTicketID",
                table: "Activities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupportTicketID",
                table: "Activities",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerCareTicketID",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "SupportTicketID",
                table: "Activities");
        }
    }
}
