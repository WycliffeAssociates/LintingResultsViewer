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