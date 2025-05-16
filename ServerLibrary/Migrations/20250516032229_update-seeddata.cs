using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ServerLibrary.Migrations
{
    /// <inheritdoc />
    public partial class updateseeddata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Applications",
                keyColumn: "ApplicationId",
                keyValue: 2);

            migrationBuilder.InsertData(
                table: "ApplicationPlans",
                columns: new[] { "Id", "ApplicationId", "Description", "MaxEmployees", "Name", "PriceMonthly", "PriceYearly" },
                values: new object[,]
                {
                    { 1, 1, "Gói tiêu chuẩn", 5, "Standard", 220000m, 2400000m },
                    { 2, 1, "Gói dành cho SME nhỏ, đầy đủ tính năng cho mọi ngành", 10, "Premium", 420000m, 4599000m },
                    { 3, 1, "Gói dành cho doanh nghiệp lớn, full tính năng bán hàng và tool marketing cho mọi ngành.", 10, "Enterprise", 800000m, 9600000m }
                });

            migrationBuilder.UpdateData(
                table: "Applications",
                keyColumn: "ApplicationId",
                keyValue: 1,
                column: "Description",
                value: "Phần mềm quản lý quan hệ khách hàng (Autuna CRM)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ApplicationPlans",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ApplicationPlans",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ApplicationPlans",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.UpdateData(
                table: "Applications",
                keyColumn: "ApplicationId",
                keyValue: 1,
                column: "Description",
                value: "Customer Relationship Management");

            migrationBuilder.InsertData(
                table: "Applications",
                columns: new[] { "ApplicationId", "CreatedDate", "Description", "ModifiedDate", "Name" },
                values: new object[] { 2, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Human Resources Management", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "HRM" });
        }
    }
}
