
using System.Text.Json;
using System.Text.Json.Serialization;
using LintingResults.Data;
using LintingResults.Services;

namespace LintingResults;

[JsonSerializable(typeof(Repo))]
[JsonSerializable(typeof(LintingResult))]
[JsonSerializable(typeof(LintingResultDBModel))]
[JsonSerializable(typeof(Dictionary<string, Dictionary<string,List<LintingResultItem>>>))]
internal partial class JSONContext: JsonSerializerContext
{
}