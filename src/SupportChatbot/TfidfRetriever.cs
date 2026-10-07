namespace SupportChatbot;

public sealed class TfidfRetriever : IRetriever
{
    private readonly IReadOnlyList<Article> _articles;
    private readonly Dictionary<string, double> _idf;
    private readonly List<Dictionary<string, double>> _vectors;
    private readonly List<double> _norms;

    public TfidfRetriever(IReadOnlyList<Article> articles)
    {
        if (articles.Count == 0)
        {
            throw new ArgumentException("The knowledge base needs at least one article.", nameof(articles));
        }

        _articles = articles;
        var documents = articles.Select(article => Tokenizer.Tokens($"{article.Title} {article.Body}")).ToList();
        _idf = InverseDocumentFrequency(documents);
        _vectors = documents.Select(tokens => Vector(TermFrequency(tokens), _idf)).ToList();
        _norms = _vectors.Select(Norm).ToList();
    }

    public IReadOnlyList<ScoredArticle> Retrieve(string query, int limit)
    {
        var queryVector = Vector(TermFrequency(Tokenizer.Tokens(query)), _idf);
        var queryNorm = Norm(queryVector);
        var scored = new List<ScoredArticle>(_articles.Count);
        for (var index = 0; index < _articles.Count; index++)
        {
            var score = queryNorm == 0 || _norms[index] == 0
                ? 0
                : Dot(queryVector, _vectors[index]) / (queryNorm * _norms[index]);
            scored.Add(new ScoredArticle(_articles[index], score));
        }

        return scored
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Article.Id, StringComparer.Ordinal)
            .Take(Math.Max(limit, 0))
            .ToList();
    }

    private static Dictionary<string, int> TermFrequency(IReadOnlyList<string> tokens)
    {
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var token in tokens)
        {
            counts[token] = counts.GetValueOrDefault(token) + 1;
        }

        return counts;
    }

    private static Dictionary<string, double> InverseDocumentFrequency(IReadOnlyList<IReadOnlyList<string>> documents)
    {
        var documentFrequency = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var document in documents)
        {
            foreach (var token in document.Distinct(StringComparer.Ordinal))
            {
                documentFrequency[token] = documentFrequency.GetValueOrDefault(token) + 1;
            }
        }

        var idf = new Dictionary<string, double>(StringComparer.Ordinal);
        var count = documents.Count;
        foreach (var (token, frequency) in documentFrequency)
        {
            idf[token] = Math.Log((count + 1.0) / (frequency + 1.0)) + 1.0;
        }

        return idf;
    }

    private static Dictionary<string, double> Vector(Dictionary<string, int> termFrequency, Dictionary<string, double> idf)
    {
        var vector = new Dictionary<string, double>(StringComparer.Ordinal);
        foreach (var (token, count) in termFrequency)
        {
            if (idf.TryGetValue(token, out var weight))
            {
                vector[token] = count * weight;
            }
        }

        return vector;
    }

    private static double Dot(Dictionary<string, double> left, Dictionary<string, double> right)
    {
        var sum = 0.0;
        foreach (var (token, weight) in left)
        {
            if (right.TryGetValue(token, out var other))
            {
                sum += weight * other;
            }
        }

        return sum;
    }

    private static double Norm(Dictionary<string, double> vector)
    {
        var sum = vector.Values.Sum(weight => weight * weight);
        return Math.Sqrt(sum);
    }
}
