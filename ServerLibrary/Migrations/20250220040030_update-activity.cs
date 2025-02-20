using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServerLibrary.Migrations
{
    /// <inheritdoc />
    public partial class updateactivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Customers_CustomerId",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_CustomerId",
                table: "Activities");

            migrationBuilder.RenameColumn(
                name: "StartTimeCustom",
                table: "Activities",
                newName: "WorkDuration");

            migrationBuilder.RenameColumn(
                name: "ModuleTypeText",
                table: "Activities",
                newName: "TagID");

            migrationBuilder.AlterColumn<string>(
                name: "ModuleType",
                table: "Activities",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndTime",
                table: "Activities",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DueDate",
                table: "Activities",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "Activities",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AccountTel",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ActivityCategory",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "BatteryStatus",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "CallDone",
                table: "Activities",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CallDuration",
                table: "Activities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CallEnd",
                table: "Activities",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CallGoalID",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CallID",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CallName",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CallRecord",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CallResult",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CallResultID",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "CallStart",
                table: "Activities",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CallTypeID",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CheckInAddress",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckInTime",
                table: "Activities",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckInType",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CheckOutAddress",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CheckOutPlace",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckOutTime",
                table: "Activities",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckinPlace",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "ContactId",
                table: "Activities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Distance",
                table: "Activities",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Duplicate",
                table: "Activities",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Activities",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EventCalendarID",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EventCheckinComment",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "EventCheckinTime",
                table: "Activities",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EventEnd",
                table: "Activities",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EventName",
                table: "Activities",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EventStart",
                table: "Activities",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCheckOutImages",
                table: "Activities",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCorrectRoute",
                table: "Activities",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Activities",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFakeGPS",
                table: "Activities",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOpen",
                table: "Activities",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Activities",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsReminder",
                table: "Activities",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRepeat",
                table: "Activities",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSendNotificationEmail",
                table: "Activities",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsStartActivity",
                table: "Activities",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Journey",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<double>(
                name: "Lat",
                table: "Activities",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Long",
                table: "Activities",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MissionName",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MissionTypeID",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "ModifiedBy",
                table: "Activities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfficeEmail",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "PartnerId",
                table: "Activities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PriorityID",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ProviderName",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "RelatedUsersID",
                table: "Activities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RemindID",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "RouteAddress",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "RoutingResultID",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "RoutingTypeID",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "RoutingTypeIDText",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SearchTagID",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "SendEmail",
                table: "Activities",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "Activities",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusID",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TagColor",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "TaskOwnerId",
                table: "Activities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TravelDistance",
                table: "Activities",
                type: "double",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "AccountTel",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "ActivityCategory",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "BatteryStatus",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "CallDone",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "CallDuration",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "CallEnd",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "CallGoalID",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "CallID",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "CallName",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "CallRecord",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "CallResult",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "CallResultID",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "CallStart",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "CallTypeID",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "CheckInAddress",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "CheckInTime",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "CheckInType",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "CheckOutAddress",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "CheckOutPlace",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "CheckOutTime",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "CheckinPlace",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "ContactId",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "Distance",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "Duplicate",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "EventCalendarID",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "EventCheckinComment",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "EventCheckinTime",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "EventEnd",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "EventName",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "EventStart",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "IsCheckOutImages",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "IsCorrectRoute",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "IsFakeGPS",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "IsOpen",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "IsReminder",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "IsRepeat",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "IsSendNotificationEmail",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "IsStartActivity",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "Journey",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "Lat",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "Long",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "MissionName",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "MissionTypeID",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "OfficeEmail",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "PartnerId",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "PriorityID",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "ProviderName",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "RelatedUsersID",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "RemindID",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "RouteAddress",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "RoutingResultID",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "RoutingTypeID",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "RoutingTypeIDText",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "SearchTagID",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "SendEmail",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "StatusID",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "TagColor",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "TaskOwnerId",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "TravelDistance",
                table: "Activities");

            migrationBuilder.RenameColumn(
                name: "WorkDuration",
                table: "Activities",
                newName: "StartTimeCustom");

            migrationBuilder.RenameColumn(
                name: "TagID",
                table: "Activities",
                newName: "ModuleTypeText");

            migrationBuilder.AlterColumn<int>(
                name: "ModuleType",
                table: "Activities",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "EndTime",
                table: "Activities",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "DueDate",
                table: "Activities",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "Activities",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Activities_CustomerId",
                table: "Activities",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Customers_CustomerId",
                table: "Activities",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
