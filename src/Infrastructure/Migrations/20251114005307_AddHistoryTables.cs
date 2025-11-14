using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HangfireJobsSys.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHistoryTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DetailsJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobExecutionLogs_History",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExecutionTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ResultMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ErrorDetails = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExecutionDurationMs = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MigratedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobExecutionLogs_History", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobPerformanceData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    JobType = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ExecutionTimeMs = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomMetrics = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPerformanceData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobPerformanceData_History",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    JobType = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ExecutionTimeMs = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomMetrics = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MigratedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPerformanceData_History", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobExecutionLogs_History_ExecutionTime",
                table: "JobExecutionLogs_History",
                column: "ExecutionTime");

            migrationBuilder.CreateIndex(
                name: "IX_JobExecutionLogs_History_JobId",
                table: "JobExecutionLogs_History",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobExecutionLogs_History_MigratedAt",
                table: "JobExecutionLogs_History",
                column: "MigratedAt");

            migrationBuilder.CreateIndex(
                name: "IX_JobPerformanceData_JobId",
                table: "JobPerformanceData",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobPerformanceData_Timestamp",
                table: "JobPerformanceData",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_JobPerformanceData_History_JobId",
                table: "JobPerformanceData_History",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobPerformanceData_History_MigratedAt",
                table: "JobPerformanceData_History",
                column: "MigratedAt");

            migrationBuilder.CreateIndex(
                name: "IX_JobPerformanceData_History_Timestamp",
                table: "JobPerformanceData_History",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "JobExecutionLogs_History");

            migrationBuilder.DropTable(
                name: "JobPerformanceData");

            migrationBuilder.DropTable(
                name: "JobPerformanceData_History");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Jobs");
        }
    }
}
