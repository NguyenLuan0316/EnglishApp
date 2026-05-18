namespace WordWave.Application.Contracts.Patterns;

public sealed record SentencePatternDto(
    int Id,
    string Sentence,
    string Type,
    string Meaning,
    string Explanation,
    IReadOnlyList<string> Examples
);
