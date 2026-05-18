using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LintingResults.Data;

public class LintingDbContext: DbContext
{
    
    public LintingDbContext(DbContextOptions<LintingDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var converter = new ValueConverter<Dictionary<string, Dictionary<string, List<LintingResultItem>>>, string>(
            v => JsonSerializer.Serialize(v, JSONContext.Default.DictionaryStringDictionaryStringListLintingResultItem),
            v => JsonSerializer.Deserialize(v, JSONContext.Default.DictionaryStringDictionaryStringListLintingResultItem));

        modelBuilder.Entity<Repo>()
            .Property(e => e.RepoName)
            .UseCollation("NOCASE");
        modelBuilder.Entity<Repo>()
            .Property(e => e.User)
            .UseCollation("NOCASE");
        modelBuilder.Entity<LintingResultDBModel>()
            .Property(e => e.LintingItems)
            .HasConversion(converter);
        modelBuilder.Entity<LintingResultDBModel>()
            .HasIndex(e => new { e.RepoId, e.CommitId })
            .IsUnique();
    }


    public DbSet<Repo> Repos { get; set; }
    public DbSet<LintingResultDBModel> LintingResults { get; set; }
}