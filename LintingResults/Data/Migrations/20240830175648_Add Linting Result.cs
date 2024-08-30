using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LintingResults.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLintingResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LintingResultDBModel",
                columns: table => new
                {
                    LintingResultDBModelId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RepoId = table.Column<int>(type: "INTEGER", nullable: false),
                    dateInserted = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LintingItems = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LintingResultDBModel", x => x.LintingResultDBModelId);
                    table.ForeignKey(
                        name: "FK_LintingResultDBModel_Repos_RepoId",
                        column: x => x.RepoId,
                        principalTable: "Repos",
                        principalColumn: "RepoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LintingResultDBModel_RepoId",
                table: "LintingResultDBModel",
                column: "RepoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LintingResultDBModel");
        }
    }
}
