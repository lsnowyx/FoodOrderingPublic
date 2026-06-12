using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class remadeOrderAssigningLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedOrderManagerId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_AssignedOrderManagerId",
                table: "Orders",
                column: "AssignedOrderManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AspNetUsers_AssignedOrderManagerId",
                table: "Orders",
                column: "AssignedOrderManagerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_AssignedOrderManagerId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_AssignedOrderManagerId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AssignedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AssignedOrderManagerId",
                table: "Orders");
        }
    }
}
