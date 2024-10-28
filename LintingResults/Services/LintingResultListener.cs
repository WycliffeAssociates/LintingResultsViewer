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
        ArgumentNullException.ThrowIfNull(connectionString, nameof(connectionString));
        _client = new ServiceBusClient(connectionString);
    }
 public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting LintingResultListener");
        _clientReceiver = _client.CreateProcessor(Topic, Subscription);
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
        var matchingRepo = lintingDbContext.Repos
            .FirstOrDefault(i => i.RepoId == lintingResult.RepoId);
        _logger.LogDebug($"Received linting result for {lintingResult.User}/{lintingResult.Repo}");
        var repoLintingItems = new Dictionary<string, Dictionary<string, List<LintingResultItem>>>();
        var result = await _httpClient.GetAsync(lintingResult.ResultsFileUrl);
        if (!result.IsSuccessStatusCode)
        {
            _logger.LogError($"Failed to download results file from {lintingResult.ResultsFileUrl}");
            repoLintingItems = null;
        }
        else
        {
            repoLintingItems = await result.Content.ReadFromJsonAsync(JSONContext.Default.DictionaryStringDictionaryStringListLintingResultItem);
        }
        if (matchingRepo == null)
        {
            var newRepo = new Repo
            {
                
                RepoId = lintingResult.RepoId,
                RepoName = lintingResult.Repo,
                User = lintingResult.User,
                LintingResults = new List<LintingResultDBModel>()
            };
            if (repoLintingItems != null)
            {

                newRepo.LintingResults.Add(
                    new()
                    {
                        dateInserted = DateTime.Now,
                        LintingItems = repoLintingItems
                    });
            }
            lintingDbContext.Repos.Add(newRepo);
        }
        else
        {
            if (repoLintingItems != null)
            {
                matchingRepo.LintingResults ??= new List<LintingResultDBModel>();
                matchingRepo.LintingResults.Add(new()
                {
                    dateInserted = DateTime.Now,
                    LintingItems = repoLintingItems
                });
            }
        }

        await lintingDbContext.SaveChangesAsync();
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
}