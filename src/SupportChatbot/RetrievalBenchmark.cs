namespace SupportChatbot;

public static class RetrievalBenchmark
{
    public static BenchmarkScore Evaluate(IRetriever retriever, IReadOnlyList<LabeledQuery> queries)
    {
        if (queries.Count == 0)
        {
            throw new ArgumentException("The benchmark needs at least one labeled query.", nameof(queries));
        }

        var recallAt1 = 0;
        var recallAt3 = 0;
        var reciprocalRank = 0.0;
        foreach (var labeled in queries)
        {
            var hits = retriever.Retrieve(labeled.Query, 3);
            var rank = 0;
            for (var index = 0; index < hits.Count; index++)
            {
                if (hits[index].Article.Id == labeled.ArticleId)
                {
                    rank = index + 1;
                    break;
                }
            }

            if (rank == 1)
            {
                recallAt1++;
            }

            if (rank is >= 1 and <= 3)
            {
                recallAt3++;
            }

            if (rank > 0)
            {
                reciprocalRank += 1.0 / rank;
            }
        }

        return new BenchmarkScore(
            recallAt1 / (double)queries.Count,
            recallAt3 / (double)queries.Count,
            reciprocalRank / queries.Count,
            queries.Count);
    }

    public static string Format(string name, BenchmarkScore score)
    {
        return string.Format(
            System.Globalization.CultureInfo.InvariantCulture,
            "{0}  recall@1={1:F3}  recall@3={2:F3}  mrr={3:F3}  n={4}",
            name,
            score.RecallAt1,
            score.RecallAt3,
            score.MeanReciprocalRank,
            score.QueryCount);
    }
}
