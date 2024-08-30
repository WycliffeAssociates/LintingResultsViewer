namespace LintingResults.Data;

public class LintingResultDBModel
{
    public int RepoId { get; set; }
    public int LintingResultDBModelId { get; set; }
    public DateTime dateInserted { get; set; }
    public Dictionary<string, Dictionary<string,List<LintingResultItem>>> LintingItems { get; set; }
}

public class LintingResultItem
{
    public string verse { get; set; }
    public string message { get; set; }
}