using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LintingResults.Migrations
{
    /// <inheritdoc />
    public partial class SeparateLintingResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LintingResultDBModel_Repos_RepoId",
                table: "LintingResultDBModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LintingResultDBModel",
                table: "LintingResultDBModel");

            migrationBuilder.RenameTable(
                name: "LintingResultDBModel",
                newName: "LintingResults");

            migrationBuilder.RenameIndex(
                name: "IX_LintingResultDBModel_RepoId",
                table: "LintingResults",
                newName: "IX_LintingResults_RepoId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LintingResults",
                table: "LintingResults",
                column: "LintingResultDBModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_LintingResults_Repos_RepoId",
                table: "LintingResults",
                column: "RepoId",
                principalTable: "Repos",
                principalColumn: "RepoId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LintingResults_Repos_RepoId",
                table: "LintingResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LintingResults",
                table: "LintingResults");

            migrationBuilder.RenameTable(
                name: "LintingResults",
                newName: "LintingResultDBModel");

            migrationBuilder.RenameIndex(
                name: "IX_LintingResults_RepoId",
                table: "LintingResultDBModel",
                newName: "IX_LintingResultDBModel_RepoId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LintingResultDBModel",
                table: "LintingResultDBModel",
                column: "LintingResultDBModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_LintingResultDBModel_Repos_RepoId",
                table: "LintingResultDBModel",
                column: "RepoId",
                principalTable: "Repos",
                principalColumn: "RepoId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
