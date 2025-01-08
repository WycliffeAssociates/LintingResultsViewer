using System.Globalization;
using CsvHelper;
using LintingResults.Data;

namespace LintingResults.Services;

public class CsvErrorDescriptionSource : IErrorDescriptionSource
{
    private Dictionary<string, string?> _errorDescriptions = [];
    private readonly string _path;
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _loadingSemaphoreSlim = new(1, 1);
    
    public CsvErrorDescriptionSource(IConfiguration configuration, ILogger<CsvErrorDescriptionSource> logger)
    {
        _path = configuration["ErrorDescriptions"] ?? "Data/ErrorCodes.csv";
        _logger = logger;
    }

    public Dictionary<string,string> GetErrorDescriptions()
    {
        _loadingSemaphoreSlim.Wait();
        if (_errorDescriptions.Count != 0)
        {
            _loadingSemaphoreSlim.Release();
            return _errorDescriptions;
        }

        try
        {
            if (!File.Exists(_path))
            {
                _logger.LogWarning("Error descriptions file not found: {Path}", _path);
                _loadingSemaphoreSlim.Release();
                return _errorDescriptions;
            }
            using var reader = new StreamReader(_path);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            _errorDescriptions =  csv.GetRecords<ErrorDescription>().ToDictionary(record => record.Error, record => $"({record.Error}) {record.Description}");
        }
        catch (Exception e)
        {
            _logger.LogError("Error loading error descriptions: {Message}", e.Message);
        }
        finally
        {
            _loadingSemaphoreSlim.Release();
        }

        return _errorDescriptions;
    }
}