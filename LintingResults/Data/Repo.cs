namespace LintingResults.Data;

public class Repo
{
    public string User { get; set; }
    public string RepoName { get; set; }
    public int RepoId { get; set; }
    public List<LintingResultDBModel> LintingResults { get; set; }
}