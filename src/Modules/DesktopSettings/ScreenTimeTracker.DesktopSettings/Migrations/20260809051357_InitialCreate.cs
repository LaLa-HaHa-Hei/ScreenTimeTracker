using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScreenTimeTracker.DesktopSettings.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DesktopSettings_LocalSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DefaultUIOpenMode = table.Column<int>(type: "INTEGER", nullable: false),
                    IsAutoStartEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSilentStartEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    Language = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DesktopSettings_LocalSettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "DesktopSettings_LocalSettings",
                columns: new[] { "Id", "DefaultUIOpenMode", "IsAutoStartEnabled", "IsSilentStartEnabled", "Language" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), 0, false, false, "en-US" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DesktopSettings_LocalSettings");
        }
    }
}
