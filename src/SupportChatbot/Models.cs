namespace SupportChatbot;

public sealed record Article(string Id, string Title, string Body);

public sealed record ScoredArticle(Article Article, double Score);

public sealed record LabeledQuery(string Query, string ArticleId);

public sealed record ChatTurn(string UserText, string AssistantText);

public sealed record BenchmarkScore(double RecallAt1, double RecallAt3, double MeanReciprocalRank, int QueryCount);
