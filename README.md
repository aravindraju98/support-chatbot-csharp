# Northline support chatbot

Object-oriented C# support chatbot for a fictional retailer, Northline Outfitters. It retrieves a policy article and answers only from that article, with the source id in the reply.

The posting this was built for asks for object-oriented programming in Java, C++, or C#. This project is the C# version: interfaces, two retriever implementations, and a chatbot that depends on those interfaces rather than a concrete class.

## What it does

- `IRetriever` is implemented by `TfidfRetriever` (term frequency–inverse document frequency, cosine similarity) and `KeywordOverlapRetriever` (shared-token baseline).
- `IResponder` / `GroundedResponder` writes the reply from the top article and cites the article id.
- `SupportChatbotEngine` asks a clarifying question when the message is blank, and abstains when nothing in the policy base matches.
- `ChatSession` keeps the turn history.
- `RetrievalBenchmark` scores recall@1, recall@3, and mean reciprocal rank on labeled questions.

## Measured result

On 20 policy articles and 26 labeled questions (20 direct questions plus 6 with distracting words from other policies):

```
tfidf    recall@1=1.000  recall@3=1.000  mrr=1.000  n=26
keyword  recall@1=0.923  recall@3=1.000  mrr=0.962  n=26
```

Those numbers come from `dotnet test` and `dotnet run -- --benchmark` in this repo. The question set is small and written for this knowledge base.

## Run

Requires the .NET 8 SDK.

```bash
dotnet test
dotnet run --project src/SupportChatbot -- --benchmark
dotnet run --project src/SupportChatbot -- My package arrived damaged. Do I send a photo?
```

## Layout

```
src/SupportChatbot/     chatbot, retrievers, benchmark, CLI
tests/                  xUnit tests for retrieval, grounding, abstain, and session history
data/articles.json      20 policy articles
data/queries.json       26 labeled questions
```
