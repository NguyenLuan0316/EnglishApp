namespace WordWave.Application.Contracts.Toeic;

public sealed record ToeicTestSummaryDto(
    int Id,
    string Title,
    string Description,
    string SourceType,
    string SourceName,
    int PartCount,
    int QuestionCount
);

public sealed record ToeicTestDetailDto(
    int Id,
    string Title,
    string Description,
    string SourceType,
    string SourceName,
    string License,
    IReadOnlyList<ToeicPartDto> Parts
);

public sealed record ToeicPartDto(
    int Id,
    int PartNumber,
    string Name,
    string Instructions,
    IReadOnlyList<ToeicPassageDto> Passages,
    IReadOnlyList<ToeicAudioDto> Audios,
    IReadOnlyList<ToeicQuestionDto> Questions
);

public sealed record ToeicQuestionDto(
    int Id,
    int PartNumber,
    int QuestionNumber,
    string Prompt,
    string QuestionText,
    string ImageUrl,
    string Difficulty,
    ToeicPassageDto? Passage,
    ToeicAudioDto? Audio,
    IReadOnlyList<ToeicAnswerOptionDto> Answers
);

public sealed record ToeicAnswerOptionDto(int Id, string Label, string AnswerText);

public sealed record ToeicPassageDto(int Id, string Title, string Content);

public sealed record ToeicAudioDto(int Id, string Url, string LocalPath, string Transcript);

public sealed record ToeicSubmitAnswerDto(int QuestionId, int AnswerId);

public sealed record ToeicSubmitRequestDto(IReadOnlyList<ToeicSubmitAnswerDto> Answers);

public sealed record ToeicSubmitResultDto(int TestId, int TotalQuestions, int AnsweredQuestions, int CorrectAnswers, decimal ScorePercent, IReadOnlyList<ToeicSubmitItemResultDto> Items);

public sealed record ToeicSubmitItemResultDto(int QuestionId, int? SelectedAnswerId, bool IsCorrect, int? CorrectAnswerId, string Explanation);

public sealed record ToeicImportLogDto(
    int Id,
    string SourceType,
    string SourceName,
    string SourceUrl,
    string Status,
    int TotalItems,
    int ImportedItems,
    int FailedItems,
    string ErrorMessage,
    string Details,
    DateTime CreatedAt
);

public sealed record ToeicImportResultDto(int? TestId, string Status, int TotalItems, int ImportedItems, int FailedItems, IReadOnlyList<string> Errors);
