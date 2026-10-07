namespace SupportChatbot;

public interface IRetriever
{
    IReadOnlyList<ScoredArticle> Retrieve(string query, int limit);
}

public interface IResponder
{
    string Compose(ScoredArticle top);
}
