using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class updatedAuditLogEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Action",
                table: "AuditLogs");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "AuditLogs",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "Endpoint",
                table: "AuditLogs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HttpMethod",
                table: "AuditLogs",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RequestBody",
                table: "AuditLogs",
                type: "nvarchar(max)",
                maxLength: 10000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponseBody",
                table: "AuditLogs",
                type: "nvarchar(max)",
                maxLength: 10000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusCode",
                table: "AuditLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Endpoint",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "HttpMethod",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "RequestBody",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "ResponseBody",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "StatusCode",
                table: "AuditLogs");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "AuditLogs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Action",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
