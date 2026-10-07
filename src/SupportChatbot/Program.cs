namespace SupportChatbot;

public static class Program
{
    public static void Main(string[] args)
    {
        var knowledge = KnowledgeBase.LoadFromSolution();
        IRetriever tfidf = new TfidfRetriever(knowledge.Articles);
        IRetriever keyword = new KeywordOverlapRetriever(knowledge.Articles);
        var chatbot = new SupportChatbotEngine(tfidf, new GroundedResponder());

        if (args.Length == 0 || args[0] == "--benchmark")
        {
            Console.WriteLine(RetrievalBenchmark.Format("tfidf", RetrievalBenchmark.Evaluate(tfidf, knowledge.Queries)));
            Console.WriteLine(RetrievalBenchmark.Format("keyword", RetrievalBenchmark.Evaluate(keyword, knowledge.Queries)));
            return;
        }

        Console.WriteLine(chatbot.Reply(string.Join(' ', args)));
    }
}
