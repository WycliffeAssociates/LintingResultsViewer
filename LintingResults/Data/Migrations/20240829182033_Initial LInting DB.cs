using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LintingResults.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialLIntingDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Repos",
                columns: table => new
                {
                    RepoId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    User = table.Column<string>(type: "TEXT", nullable: false),
                    RepoName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Repos", x => x.RepoId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Repos");
        }
    }
}
