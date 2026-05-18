using System.Text.Json;
using Azure.Messaging.ServiceBus;
using LintingResults.Data;

namespace LintingResults.Services;

public class LintingResultListener: IHostedService
{
    private readonly ILogger<LintingResultListener> _logger;
    private ServiceBusProcessor _clientReceiver;
    private readonly ServiceBusClient _client;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly HttpClient _httpClient = new();

    private const string Topic = "LintingResult";
    private const string Subscription = "ResultsUI";

    public LintingResultListener(ILogger<LintingResultListener> logger, IServiceScopeFactory scopeFactory, IConfiguration config)
    {
        _logger = logger;
        _serviceScopeFactory = scopeFactory;
        var connectionString = config.Get<ConfigurationModel>()?.ServiceBusConnectionString;
        ArgumentNullException.ThrowIfNull(connectionString);
        _client = new ServiceBusClient(connectionString);
    }
 public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting LintingResultListener");
        _clientReceiver = _client.CreateProcessor(Topic, Subscription, new ServiceBusProcessorOptions() {MaxConcurrentCalls = 1}); // We might want to scale this later but this eliminates race conditions
        _clientReceiver.ProcessMessageAsync += ProcessMessage;
        _clientReceiver.ProcessErrorAsync += args =>
        {
            _logger.LogError(args.Exception, "Error processing message");
            return Task.CompletedTask;
        };
        await _clientReceiver.StartProcessingAsync(cancellationToken);
    }

    private async Task ProcessMessage(ProcessMessageEventArgs arg)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var lintingDbContext = scope.ServiceProvider.GetRequiredService<LintingDbContext>();
        var lintingResult = JsonSerializer.Deserialize(arg.Message.Body, JSONContext.Default.LintingResult);
        _logger.LogDebug($"Received linting result for {lintingResult.User}/{lintingResult.Repo}");

        using var response = await _httpClient.GetAsync(lintingResult.ResultsFileUrl);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"Failed to download results file from {lintingResult.ResultsFileUrl}");
            return;
        }
        var repoLintingItems = await response.Content.ReadFromJsonAsync(JSONContext.Default.DictionaryStringDictionaryStringListLintingResultItem);
        if (repoLintingItems == null)
        {
            _logger.LogError($"Failed to deserialize linting items from {lintingResult.ResultsFileUrl}");
            return;
        }

        if (lintingResult.CommitId != null)
        {
            var existingForCommit = lintingDbContext.LintingResults
                .FirstOrDefault(r => r.RepoId == lintingResult.RepoId && r.CommitId == lintingResult.CommitId);
            if (existingForCommit != null)
            {
                MergeInto(existingForCommit.LintingItems, repoLintingItems);
                lintingDbContext.Entry(existingForCommit).Property(x => x.LintingItems).IsModified = true;
                await lintingDbContext.SaveChangesAsync();
                return;
            }
        }

        var matchingRepo = lintingDbContext.Repos.FirstOrDefault(i => i.RepoId == lintingResult.RepoId);
        if (matchingRepo == null)
        {
            lintingDbContext.Repos.Add(new Repo
            {
                RepoId = lintingResult.RepoId,
                RepoName = lintingResult.Repo,
                User = lintingResult.User,
                LintingResults = new List<LintingResultDBModel>
                {
                    new() { dateInserted = DateTime.Now, CommitId = lintingResult.CommitId, LintingItems = repoLintingItems }
                }
            });
        }
        else
        {
            matchingRepo.LintingResults ??= new List<LintingResultDBModel>();
            matchingRepo.LintingResults.Add(new()
            {
                dateInserted = DateTime.Now,
                CommitId = lintingResult.CommitId,
                LintingItems = repoLintingItems
            });
        }

        await lintingDbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Merges linting results
    /// </summary>
    /// <param name="existing">The already existing result</param>
    /// <param name="incoming">The new one we have coming in</param>
    /// <remarks>This will merge things into a chapter -> verse hierarchy while not inserting duplicates</remarks>
    private static void MergeInto(
        Dictionary<string, Dictionary<string, List<LintingResultItem>>>? existing,
        Dictionary<string, Dictionary<string, List<LintingResultItem>>>? incoming)
    {
        if (incoming == null || incoming.Count == 0)
        {
            return;
        }
        
        existing ??= new Dictionary<string, Dictionary<string, List<LintingResultItem>>>();
        
        foreach (var (book, incomingChapters) in incoming)
        {
            if (!existing.TryGetValue(book, out var existingChapters))
            {
                existing[book] = incomingChapters;
                continue;
            }
            foreach (var (chapter, incomingItems) in incomingChapters)
            {
                if (!existingChapters.TryGetValue(chapter, out var existingItems))
                {
                    existingChapters[chapter] = incomingItems;
                    continue;
                }
                var existingKeys = existingItems.Select(i => (i.verse, i.errorId)).ToHashSet();
                foreach (var item in incomingItems)
                {
                    if (existingKeys.Add((item.verse, item.errorId)))
                    {
                        existingItems.Add(item);
                    }
                }
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping LintingResultListener");
        await _clientReceiver.CloseAsync(cancellationToken);
    }
}

public class LintingResult
{
    public int RepoId { get; set; }
    public string User { get; set; }
    public string Repo { get; set; }
    public string ResultsFileUrl { get; set; }
    public string? CommitId { get; set; }
}