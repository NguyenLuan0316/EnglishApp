namespace WordWave.Application.Contracts.Patterns;

public sealed record SentencePatternDto(
    int Id,
    string Sentence,
    string Meaning,
    string Explanation,
    IReadOnlyList<string> Examples
);
