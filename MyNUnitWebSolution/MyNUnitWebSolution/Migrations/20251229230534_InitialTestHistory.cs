using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyNUnitWebSolution.Migrations
{
    /// <inheritdoc />
    public partial class InitialTestHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TestRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssemblyRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TestRunId = table.Column<int>(type: "INTEGER", nullable: false),
                    AssemblyName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssemblyRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssemblyRuns_TestRuns_TestRunId",
                        column: x => x.TestRunId,
                        principalTable: "TestRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestOutputLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AssemblyRunId = table.Column<int>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    Text = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestOutputLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestOutputLines_AssemblyRuns_AssemblyRunId",
                        column: x => x.AssemblyRunId,
                        principalTable: "AssemblyRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssemblyRuns_TestRunId",
                table: "AssemblyRuns",
                column: "TestRunId");

            migrationBuilder.CreateIndex(
                name: "IX_TestOutputLines_AssemblyRunId",
                table: "TestOutputLines",
                column: "AssemblyRunId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestOutputLines");

            migrationBuilder.DropTable(
                name: "AssemblyRuns");

            migrationBuilder.DropTable(
                name: "TestRuns");
        }
    }
}
