namespace SupportChatbot.Tests;

public class ChatbotTests
{
    private readonly KnowledgeBase _knowledge = KnowledgeBase.LoadFromSolution();

    [Fact]
    public void TfidfRecallAt3CoversEveryLabeledQuestion()
    {
        IRetriever retriever = new TfidfRetriever(_knowledge.Articles);
        var score = RetrievalBenchmark.Evaluate(retriever, _knowledge.Queries);

        Assert.Equal(26, score.QueryCount);
        Assert.Equal(1.0, score.RecallAt3);
        Assert.True(score.RecallAt1 >= 0.85, $"recall@1 was {score.RecallAt1:F3}");
    }

    [Fact]
    public void TfidfBeatsKeywordOverlapOnReciprocalRank()
    {
        var tfidf = RetrievalBenchmark.Evaluate(new TfidfRetriever(_knowledge.Articles), _knowledge.Queries);
        var keyword = RetrievalBenchmark.Evaluate(new KeywordOverlapRetriever(_knowledge.Articles), _knowledge.Queries);

        Assert.True(tfidf.MeanReciprocalRank > keyword.MeanReciprocalRank);
    }

    [Fact]
    public void ReplyCitesTheDamagedItemPolicy()
    {
        var chatbot = new SupportChatbotEngine(new TfidfRetriever(_knowledge.Articles), new GroundedResponder());
        var reply = chatbot.Reply("My package arrived damaged. Do I send a photo for a replacement?");

        Assert.Contains("source: returns-damaged", reply);
        Assert.Contains("waives the return shipping fee", reply);
    }

    [Fact]
    public void EmptyQuestionAsksForMoreDetail()
    {
        var chatbot = new SupportChatbotEngine(new TfidfRetriever(_knowledge.Articles), new GroundedResponder());

        Assert.Equal(SupportChatbotEngine.ClarifyMessage, chatbot.Reply("  "));
    }

    [Fact]
    public void OffTopicQuestionAbstains()
    {
        var chatbot = new SupportChatbotEngine(new TfidfRetriever(_knowledge.Articles), new GroundedResponder());
        var reply = chatbot.Reply("What is the capital of France?");

        Assert.Equal(SupportChatbotEngine.AbstainMessage, reply);
    }

    [Fact]
    public void SessionKeepsTheTurnHistory()
    {
        var chatbot = new SupportChatbotEngine(new TfidfRetriever(_knowledge.Articles), new GroundedResponder());
        var session = new ChatSession(chatbot);

        session.Say("Are the stores open on Thanksgiving?");
        session.Say(" ");

        Assert.Equal(2, session.Turns.Count);
        Assert.Contains("source: store-hours", session.Turns[0].AssistantText);
        Assert.Equal(SupportChatbotEngine.ClarifyMessage, session.Turns[1].AssistantText);
    }
}
