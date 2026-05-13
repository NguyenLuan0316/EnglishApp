namespace WordWave.Application.Contracts.Vocabulary;

public sealed record VocabularyQuery(
    string? Level,
    string? Topic,
    string? Search,
    int Page = 1,
    int Limit = 20
);
