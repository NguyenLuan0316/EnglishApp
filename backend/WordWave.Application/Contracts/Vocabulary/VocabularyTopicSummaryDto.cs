namespace WordWave.Application.Contracts.Vocabulary;

public sealed record VocabularyTopicSummaryDto(string Topic, int Count, IReadOnlyList<string> Levels);
