namespace LintingResults.Helpers
{
    internal static class BibleBookOrder
    {
        // Canonical list of Bible book abbreviations used for ordering throughout the app
        internal static readonly IReadOnlyList<string> Abbreviations = new List<string>
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

        // Precomputed lookup map for fast index retrieval (case-insensitive)
        private static readonly Dictionary<string, int> IndexMap = Abbreviations
            .Select((book, idx) => (book, idx))
            .ToDictionary(x => x.book, x => x.idx, StringComparer.OrdinalIgnoreCase);

        // Get the canonical index for a book abbreviation. Unknown books return int.MaxValue so they sort last.
        internal static int GetIndex(string? book)
        {
            if (string.IsNullOrEmpty(book)) return int.MaxValue;
            if (IndexMap.TryGetValue(book, out var idx)) return idx;
            return int.MaxValue;
        }
    }
}
