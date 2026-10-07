namespace SupportChatbot;

/// <summary>
/// Baseline retriever. Shared token count, with no inverse-document-frequency weighting.
/// </summary>
public sealed class KeywordOverlapRetriever : IRetriever
{
    private readonly IReadOnlyList<Article> _articles;
    private readonly List<HashSet<string>> _documentTokens;

    public KeywordOverlapRetriever(IReadOnlyList<Article> articles)
    {
        _articles = articles;
        _documentTokens = articles
            .Select(article => Tokenizer.Tokens($"{article.Title} {article.Body}").ToHashSet(StringComparer.Ordinal))
            .ToList();
    }

    public IReadOnlyList<ScoredArticle> Retrieve(string query, int limit)
    {
        var queryTokens = Tokenizer.Tokens(query);
        var scored = new List<ScoredArticle>(_articles.Count);
        for (var index = 0; index < _articles.Count; index++)
        {
            var score = queryTokens.Count == 0
                ? 0
                : queryTokens.Count(token => _documentTokens[index].Contains(token)) / (double)queryTokens.Count;
            scored.Add(new ScoredArticle(_articles[index], score));
        }

        return scored
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Article.Id, StringComparer.Ordinal)
            .Take(Math.Max(limit, 0))
            .ToList();
    }
}
