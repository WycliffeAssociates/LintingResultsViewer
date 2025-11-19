using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;
using LintingResults.Components;
using LintingResults.Components.Account;
using LintingResults.Components.Pages;
using LintingResults.Data;
using LintingResults.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddFluentUIComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDbContextFactory<LintingDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDataGridEntityFrameworkAdapter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
builder.Services.AddSingleton<IErrorDescriptionSource, CsvErrorDescriptionSource>();
builder.Services.AddHostedService<LintingResultListener>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

// API endpoint to get the most recent linting result for a repository
app.MapGet("/api/linting/{username}/{reponame}", async (string username, string reponame, IDbContextFactory<LintingDbContext> contextFactory) =>
{
    using var context = contextFactory.CreateDbContext();
    var repo = await context.Repos
        .Include(r => r.LintingResults)
        .FirstOrDefaultAsync(r => r.User == username && r.RepoName == reponame);
    
    if (repo == null || repo.LintingResults == null || !repo.LintingResults.Any())
    {
        return Results.NotFound(new { message = $"No linting results found for {username}/{reponame}" });
    }
    
    var mostRecentResult = repo.LintingResults
        .OrderByDescending(lr => lr.dateInserted)
        .First();
    
    return Results.Ok(mostRecentResult);
});

// API endpoint to download linting results as CSV
app.MapGet("/api/linting/csv/{scanId:int}", async (int scanId, IDbContextFactory<LintingDbContext> contextFactory) =>
{
    using var context = contextFactory.CreateDbContext();
    var result = await context.LintingResults.FirstOrDefaultAsync(lr => lr.LintingResultDBModelId == scanId);
    
    if (result == null)
    {
        return Results.NotFound(new { message = $"No linting result found for scan ID {scanId}" });
    }
    
    var repo = await context.Repos.FirstOrDefaultAsync(r => r.RepoId == result.RepoId);
    
    // Bible book order (same as displayed on the scan details page)
    var bibleBookAbbreviations = new List<string>
    {
        // Old Testament
        "GEN", "EXO", "LEV", "NUM", "DEU", "JOS", "JDG", "RUT", "1SA", "2SA",
        "1KI", "2KI", "1CH", "2CH", "EZR", "NEH", "EST", "JOB", "PSA", "PRO",
        "ECC", "SNG", "ISA", "JER", "LAM", "EZK", "DAN", "HOS", "JOL", "AMO",
        "OBA", "JON", "MIC", "NAM", "HAB", "ZEP", "HAG", "ZEC", "MAL",
        // New Testament
        "MAT", "MRK", "LUK", "JHN", "ACT", "ROM", "1CO", "2CO", "GAL", "EPH",
        "PHP", "COL", "1TH", "2TH", "1TI", "2TI", "TIT", "PHM", "HEB",
        "JAS", "1PE", "2PE", "1JN", "2JN", "3JN", "JUD", "REV"
    };
    
    // Order books by biblical order
    var orderedLintingItems = result.LintingItems
        .OrderBy(i => bibleBookAbbreviations.IndexOf(i.Key))
        .ToDictionary(i => i.Key, i => i.Value);
    
    using var memoryStream = new MemoryStream();
    using var writer = new StreamWriter(memoryStream);
    using var csv = new CsvHelper.CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture);
    
    // Write CSV header
    csv.WriteField("Book");
    csv.WriteField("Chapter");
    csv.WriteField("Verse");
    csv.WriteField("Error ID");
    csv.WriteField("Message");
    csv.NextRecord();
    
    // Write CSV data in biblical book order
    foreach (var book in orderedLintingItems)
    {
        foreach (var chapter in book.Value)
        {
            foreach (var item in chapter.Value)
            {
                csv.WriteField(book.Key);
                csv.WriteField(chapter.Key);
                csv.WriteField(item.verse);
                csv.WriteField(item.errorId);
                csv.WriteField(item.message);
                csv.NextRecord();
            }
        }
    }
    
    await writer.FlushAsync();
    memoryStream.Position = 0;
    
    var fileName = repo != null 
        ? $"{repo.User}_{repo.RepoName}_{result.dateInserted:yyyy-MM-dd_HH-mm-ss}.csv"
        : $"linting_results_{scanId}.csv";
    
    return Results.File(memoryStream.ToArray(), "text/csv", fileName);
});

// Apply any pending migrations on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
    var lintingContext = services.GetRequiredService<LintingDbContext>();
    lintingContext.Database.Migrate();
}

app.Run();