using System.Text.RegularExpressions;

namespace SupportChatbot;

public static class Tokenizer
{
    private static readonly HashSet<string> Stopwords = new(StringComparer.Ordinal)
    {
        "a", "an", "the", "to", "of", "and", "for", "is", "my", "i", "you", "your",
        "we", "our", "it", "on", "in", "at", "do", "does", "can", "with", "or", "be",
        "if", "from", "that", "this", "after", "before", "once", "still", "only",
        "when", "what", "how", "who", "are", "me", "us", "did", "was", "were",
    };

    public static IReadOnlyList<string> Tokens(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Array.Empty<string>();
        }

        var matches = Regex.Matches(text.ToLowerInvariant(), "[a-z0-9]+");
        var tokens = new List<string>(matches.Count);
        foreach (Match match in matches)
        {
            if (!Stopwords.Contains(match.Value))
            {
                tokens.Add(match.Value);
            }
        }

        return tokens;
    }
}
