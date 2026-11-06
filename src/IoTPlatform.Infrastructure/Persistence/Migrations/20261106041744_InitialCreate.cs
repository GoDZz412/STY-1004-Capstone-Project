using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceAssets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DeviceType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    OperatingMin = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OperatingMax = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OperatingUnit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InstalledOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceAssets", x => x.Id);
                    table.CheckConstraint("CK_DeviceAssets_OperatingRange_Order", "CAST(OperatingMax AS REAL) > CAST(OperatingMin AS REAL)");
                    table.CheckConstraint("CK_DeviceAssets_OperatingRange_Positive", "CAST(OperatingMin AS REAL) >= 0");
                });

            migrationBuilder.CreateTable(
                name: "AlertRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MetricType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Operator = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Threshold = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertRules_DeviceAssets_DeviceAssetId",
                        column: x => x.DeviceAssetId,
                        principalTable: "DeviceAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaintenanceType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Technician = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    PerformedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceLogs_DeviceAssets_DeviceAssetId",
                        column: x => x.DeviceAssetId,
                        principalTable: "DeviceAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TelemetryMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MetricType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RecordedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelemetryMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TelemetryMetrics_DeviceAssets_DeviceAssetId",
                        column: x => x.DeviceAssetId,
                        principalTable: "DeviceAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertRules_DeviceAssetId_MetricType",
                table: "AlertRules",
                columns: new[] { "DeviceAssetId", "MetricType" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAssets_SerialNumber",
                table: "DeviceAssets",
                column: "SerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAssets_Status",
                table: "DeviceAssets",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceLogs_DeviceAssetId_PerformedOnUtc",
                table: "MaintenanceLogs",
                columns: new[] { "DeviceAssetId", "PerformedOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryMetrics_DeviceAssetId_MetricType",
                table: "TelemetryMetrics",
                columns: new[] { "DeviceAssetId", "MetricType" });

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryMetrics_DeviceAssetId_RecordedAtUtc",
                table: "TelemetryMetrics",
                columns: new[] { "DeviceAssetId", "RecordedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertRules");

            migrationBuilder.DropTable(
                name: "MaintenanceLogs");

            migrationBuilder.DropTable(
                name: "TelemetryMetrics");

            migrationBuilder.DropTable(
                name: "DeviceAssets");
        }
    }
}
