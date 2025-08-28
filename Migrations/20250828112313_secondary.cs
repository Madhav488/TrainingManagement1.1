using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainingManagement1.Migrations
{
    /// <inheritdoc />
    public partial class secondary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Batch_CourseCalendar_CalendarId",
                table: "Batch");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOn",
                table: "Batch",
                type: "datetime",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true,
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Batch",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Batch",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Batch_CourseCalendar_CalendarId",
                table: "Batch",
                column: "CalendarId",
                principalTable: "CourseCalendar",
                principalColumn: "CalendarId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Batch_CourseCalendar_CalendarId",
                table: "Batch");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Batch");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Batch");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOn",
                table: "Batch",
                type: "datetime",
                nullable: true,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AddForeignKey(
                name: "FK_Batch_CourseCalendar_CalendarId",
                table: "Batch",
                column: "CalendarId",
                principalTable: "CourseCalendar",
                principalColumn: "CalendarId");
        }
    }
}
