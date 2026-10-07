using System.Text.Json;

namespace SupportChatbot;

public sealed class KnowledgeBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public KnowledgeBase(IReadOnlyList<Article> articles, IReadOnlyList<LabeledQuery> queries)
    {
        Articles = articles;
        Queries = queries;
    }

    public IReadOnlyList<Article> Articles { get; }

    public IReadOnlyList<LabeledQuery> Queries { get; }

    public static KnowledgeBase LoadFromSolution()
    {
        var root = FindSolutionRoot();
        return Load(
            Path.Combine(root, "data", "articles.json"),
            Path.Combine(root, "data", "queries.json"));
    }

    public static KnowledgeBase Load(string articlesPath, string queriesPath)
    {
        var articles = JsonSerializer.Deserialize<List<Article>>(File.ReadAllText(articlesPath), JsonOptions)
            ?? throw new InvalidDataException($"No articles in {articlesPath}.");
        var queries = JsonSerializer.Deserialize<List<LabeledQuery>>(File.ReadAllText(queriesPath), JsonOptions)
            ?? throw new InvalidDataException($"No queries in {queriesPath}.");
        if (articles.Count == 0 || queries.Count == 0)
        {
            throw new InvalidDataException("Articles and labeled queries are both required.");
        }

        return new KnowledgeBase(articles, queries);
    }

    public static string FindSolutionRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "SupportChatbot.sln")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not find SupportChatbot.sln above the application directory.");
    }
}
