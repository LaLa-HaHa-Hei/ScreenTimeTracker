using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ScreenTimeTracker.ScreenTime.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScreenTime_AppCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false),
                    IconPath = table.Column<string>(type: "TEXT", nullable: true),
                    IconPathLastUpdatedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    IsSystem = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreenTime_AppCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScreenTime_UserSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AppTracking_ActiveUsageSessionAutoSaveInterval = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    AppTracking_IconDirectory = table.Column<string>(type: "TEXT", nullable: false),
                    AppTracking_MetadataStaleThreshold = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    AppTracking_MinValidUsageSessionDuration = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    AppTracking_UsageSessionMergeTolerance = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    AppTracking_UsageSessionOptimizationInterval = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    IdleDetection_InactivityThreshold = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    IdleDetection_IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    IdleDetection_PollingInterval = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    TimeBoundary_DayCutoffHour = table.Column<int>(type: "INTEGER", nullable: false),
                    WebsiteTracking_ActiveUsageSessionAutoSaveInterval = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    WebsiteTracking_IconDirectory = table.Column<string>(type: "TEXT", nullable: false),
                    WebsiteTracking_MinValidUsageSessionDuration = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    WebsiteTracking_UsageSessionMergeTolerance = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    WebsiteTracking_UsageSessionOptimizationInterval = table.Column<TimeSpan>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreenTime_UserSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScreenTime_WebsiteCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false),
                    IconPath = table.Column<string>(type: "TEXT", nullable: true),
                    IconPathLastUpdatedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    IsSystem = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreenTime_WebsiteCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScreenTime_Apps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false),
                    ProcessName = table.Column<string>(type: "TEXT", nullable: false),
                    AllowMetadataAutoRefresh = table.Column<bool>(type: "INTEGER", nullable: false),
                    MetadataLastRefreshedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    AppCategoryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExecutablePath = table.Column<string>(type: "TEXT", nullable: true),
                    IconPath = table.Column<string>(type: "TEXT", nullable: true),
                    IconPathLastUpdatedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    IsSystem = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreenTime_Apps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScreenTime_Apps_ScreenTime_AppCategories_AppCategoryId",
                        column: x => x.AppCategoryId,
                        principalTable: "ScreenTime_AppCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScreenTime_Websites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false),
                    Host = table.Column<string>(type: "TEXT", nullable: false),
                    AllowMetadataAutoRefresh = table.Column<bool>(type: "INTEGER", nullable: false),
                    MetadataLastRefreshedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    WebsiteCategoryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IconPath = table.Column<string>(type: "TEXT", nullable: true),
                    IconPathLastUpdatedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    IsSystem = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreenTime_Websites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScreenTime_Websites_ScreenTime_WebsiteCategories_WebsiteCategoryId",
                        column: x => x.WebsiteCategoryId,
                        principalTable: "ScreenTime_WebsiteCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScreenTime_AppUsageSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AppId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StartTime = table.Column<long>(type: "INTEGER", nullable: false),
                    EndTime = table.Column<long>(type: "INTEGER", nullable: false),
                    IsOptimized = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreenTime_AppUsageSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScreenTime_AppUsageSessions_ScreenTime_Apps_AppId",
                        column: x => x.AppId,
                        principalTable: "ScreenTime_Apps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScreenTime_WebsiteUsageSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    WebsiteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StartTime = table.Column<long>(type: "INTEGER", nullable: false),
                    EndTime = table.Column<long>(type: "INTEGER", nullable: false),
                    IsOptimized = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreenTime_WebsiteUsageSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScreenTime_WebsiteUsageSessions_ScreenTime_Websites_WebsiteId",
                        column: x => x.WebsiteId,
                        principalTable: "ScreenTime_Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ScreenTime_AppCategories",
                columns: new[] { "Id", "Color", "IconPath", "IconPathLastUpdatedAt", "IsSystem", "Name" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), "#8E8E93", null, -62135596800000L, true, "Uncategorized" });

            migrationBuilder.InsertData(
                table: "ScreenTime_WebsiteCategories",
                columns: new[] { "Id", "Color", "IconPath", "IconPathLastUpdatedAt", "IsSystem", "Name" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), "#8E8E93", null, -62135596800000L, true, "Uncategorized" });

            migrationBuilder.InsertData(
                table: "ScreenTime_Apps",
                columns: new[] { "Id", "AllowMetadataAutoRefresh", "AppCategoryId", "Color", "ExecutablePath", "IconPath", "IconPathLastUpdatedAt", "IsSystem", "MetadataLastRefreshedAt", "Name", "ProcessName" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), false, new Guid("00000000-0000-0000-0000-000000000001"), "#C7C7CC", null, null, -62135596800000L, true, -62135596800000L, "Idle", "Idle" },
                    { new Guid("00000000-0000-0000-0000-000000000002"), false, new Guid("00000000-0000-0000-0000-000000000001"), "#636366", null, null, -62135596800000L, true, -62135596800000L, "Unknown", "Unknown" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScreenTime_AppCategories_Name",
                table: "ScreenTime_AppCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScreenTime_Apps_AppCategoryId",
                table: "ScreenTime_Apps",
                column: "AppCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ScreenTime_Apps_ProcessName",
                table: "ScreenTime_Apps",
                column: "ProcessName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScreenTime_AppUsageSessions_AppId_EndTime",
                table: "ScreenTime_AppUsageSessions",
                columns: new[] { "AppId", "EndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ScreenTime_AppUsageSessions_EndTime",
                table: "ScreenTime_AppUsageSessions",
                column: "EndTime");

            migrationBuilder.CreateIndex(
                name: "IX_ScreenTime_AppUsageSessions_StartTime",
                table: "ScreenTime_AppUsageSessions",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_ScreenTime_WebsiteCategories_Name",
                table: "ScreenTime_WebsiteCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScreenTime_Websites_Host",
                table: "ScreenTime_Websites",
                column: "Host",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScreenTime_Websites_WebsiteCategoryId",
                table: "ScreenTime_Websites",
                column: "WebsiteCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ScreenTime_WebsiteUsageSessions_EndTime",
                table: "ScreenTime_WebsiteUsageSessions",
                column: "EndTime");

            migrationBuilder.CreateIndex(
                name: "IX_ScreenTime_WebsiteUsageSessions_StartTime",
                table: "ScreenTime_WebsiteUsageSessions",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_ScreenTime_WebsiteUsageSessions_WebsiteId_EndTime",
                table: "ScreenTime_WebsiteUsageSessions",
                columns: new[] { "WebsiteId", "EndTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScreenTime_AppUsageSessions");

            migrationBuilder.DropTable(
                name: "ScreenTime_UserSettings");

            migrationBuilder.DropTable(
                name: "ScreenTime_WebsiteUsageSessions");

            migrationBuilder.DropTable(
                name: "ScreenTime_Apps");

            migrationBuilder.DropTable(
                name: "ScreenTime_Websites");

            migrationBuilder.DropTable(
                name: "ScreenTime_AppCategories");

            migrationBuilder.DropTable(
                name: "ScreenTime_WebsiteCategories");
        }
    }
}
