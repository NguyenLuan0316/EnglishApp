namespace WordWave.Application.Contracts.Grammar;

public sealed record GrammarLessonDto(
    int Id,
    string Title,
    string Level,
    string Description,
    string Formula,
    string Tips,
    IReadOnlyList<GrammarExampleDto> Examples
);
