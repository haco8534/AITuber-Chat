using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class SentenceSplitter
{
    private static readonly Regex Pattern = new Regex(@"[^。！？\n]+[。！？]?", RegexOptions.Compiled);

    public static List<string> Split(string text)
    {
        var result = new List<string>();
        if (string.IsNullOrEmpty(text)) return result;

        foreach (Match m in Pattern.Matches(text))
        {
            var s = m.Value.Trim();
            if (s.Length > 0) result.Add(s);
        }
        return result;
    }
}
