using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScreenTimeTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcessInfos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Alias = table.Column<string>(type: "TEXT", nullable: true),
                    AutoUpdate = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastAutoUpdated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExecutablePath = table.Column<string>(type: "TEXT", nullable: true),
                    IconPath = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ActivityIntervals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProcessInfoEntityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DurationMilliseconds = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityIntervals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityIntervals_ProcessInfos_ProcessInfoEntityId",
                        column: x => x.ProcessInfoEntityId,
                        principalTable: "ProcessInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HourlySummaries",
                columns: table => new
                {
                    ProcessInfoEntityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Hour = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalDurationMilliseconds = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HourlySummaries", x => new { x.ProcessInfoEntityId, x.Hour });
                    table.ForeignKey(
                        name: "FK_HourlySummaries_ProcessInfos_ProcessInfoEntityId",
                        column: x => x.ProcessInfoEntityId,
                        principalTable: "ProcessInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityIntervals_ProcessInfoEntityId",
                table: "ActivityIntervals",
                column: "ProcessInfoEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityIntervals_Timestamp",
                table: "ActivityIntervals",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessInfos_Name",
                table: "ProcessInfos",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityIntervals");

            migrationBuilder.DropTable(
                name: "HourlySummaries");

            migrationBuilder.DropTable(
                name: "ProcessInfos");
        }
    }
}
