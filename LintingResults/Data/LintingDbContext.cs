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

        modelBuilder.Entity<LintingResultDBModel>()
            .Property(e => e.LintingItems)
            .HasConversion(converter);
    }


    public DbSet<Repo> Repos { get; set; }
    public DbSet<LintingResultDBModel> LintingResults { get; set; }
}