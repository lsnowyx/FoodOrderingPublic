using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class mapImplementation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeliveryTrackingSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartLatitude = table.Column<decimal>(type: "decimal(18,12)", precision: 18, scale: 12, nullable: false),
                    StartLongitude = table.Column<decimal>(type: "decimal(18,12)", precision: 18, scale: 12, nullable: false),
                    DestinationLatitude = table.Column<decimal>(type: "decimal(18,12)", precision: 18, scale: 12, nullable: false),
                    DestinationLongitude = table.Column<decimal>(type: "decimal(18,12)", precision: 18, scale: 12, nullable: false),
                    CurrentLatitude = table.Column<decimal>(type: "decimal(18,12)", precision: 18, scale: 12, nullable: false),
                    CurrentLongitude = table.Column<decimal>(type: "decimal(18,12)", precision: 18, scale: 12, nullable: false),
                    Progress = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ArrivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationSeconds = table.Column<int>(type: "int", nullable: false),
                    UpdateIntervalSeconds = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryTrackingSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryTrackingSessions_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryTrackingSessions_OrderId",
                table: "DeliveryTrackingSessions",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryTrackingSessions_OrderId_InProgress",
                table: "DeliveryTrackingSessions",
                column: "OrderId",
                unique: true,
                filter: "[Status] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryTrackingSessions");
        }
    }
}
