using System.Collections.Frozen;
using LintingResults.Data;

namespace LintingResults.Services;

public interface IErrorDescriptionSource
{
    public Dictionary<string, string> GetErrorDescriptions();
}