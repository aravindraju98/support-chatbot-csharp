namespace SupportChatbot;

public sealed class GroundedResponder : IResponder
{
    public string Compose(ScoredArticle top)
    {
        var article = top.Article;
        return $"Here's what our policy says on {article.Title}: {article.Body} (source: {article.Id})";
    }
}

public sealed class SupportChatbotEngine
{
    public const string ClarifyMessage =
        "Please tell me a bit more about the order, return, shipping, or product you need help with.";

    public const string AbstainMessage =
        "I don't have a Northline policy that matches that question. Try orders, returns, shipping, billing, or product care.";

    private readonly IRetriever _retriever;
    private readonly IResponder _responder;

    public SupportChatbotEngine(IRetriever retriever, IResponder responder)
    {
        _retriever = retriever;
        _responder = responder;
    }

    public string Reply(string? userText)
    {
        if (string.IsNullOrWhiteSpace(userText))
        {
            return ClarifyMessage;
        }

        var hits = _retriever.Retrieve(userText, 3);
        if (hits.Count == 0 || hits[0].Score <= 0)
        {
            return AbstainMessage;
        }

        return _responder.Compose(hits[0]);
    }
}

public sealed class ChatSession
{
    private readonly SupportChatbotEngine _engine;
    private readonly List<ChatTurn> _turns = new();

    public ChatSession(SupportChatbotEngine engine)
    {
        _engine = engine;
    }

    public IReadOnlyList<ChatTurn> Turns => _turns;

    public string Say(string userText)
    {
        var answer = _engine.Reply(userText);
        _turns.Add(new ChatTurn(userText, answer));
        return answer;
    }
}
