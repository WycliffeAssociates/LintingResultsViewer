using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LintingResults.Data.Migrations
{
    /// <inheritdoc />
    public partial class UniqueRepoIdCommitId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LintingResults_RepoId",
                table: "LintingResults");

            migrationBuilder.CreateIndex(
                name: "IX_LintingResults_RepoId_CommitId",
                table: "LintingResults",
                columns: new[] { "RepoId", "CommitId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LintingResults_RepoId_CommitId",
                table: "LintingResults");

            migrationBuilder.CreateIndex(
                name: "IX_LintingResults_RepoId",
                table: "LintingResults",
                column: "RepoId");
        }
    }
}
