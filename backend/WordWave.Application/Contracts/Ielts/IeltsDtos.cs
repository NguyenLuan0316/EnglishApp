using System.Text.Json;

namespace WordWave.Application.Contracts.Ielts;

public sealed record IeltsTestSummaryDto(
    int Id,
    string Title,
    string Description,
    string SourceType,
    string SourceName,
    int QuestionCount,
    DateTime CreatedAt,
    IeltsAttemptSummaryDto? Attempt
);

public sealed record IeltsAttemptSummaryDto(
    int Id,
    bool Started,
    bool IsSubmitted,
    decimal? OverallBand,
    int AnsweredCount,
    DateTime UpdatedAt,
    DateTime? SubmittedAt
);

public sealed record IeltsTestDetailDto(
    int Id,
    string Title,
    string Description,
    string SourceType,
    string SourceName,
    int QuestionCount,
    DateTime CreatedAt,
    JsonElement TestData,
    IeltsAttemptDto? Attempt
);

public sealed record IeltsAttemptDto(
    int Id,
    int TestId,
    string LearnerId,
    JsonElement StateData,
    JsonElement ResultData,
    bool IsSubmitted,
    decimal? OverallBand,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? SubmittedAt
);

public sealed record IeltsSaveAttemptRequestDto(string LearnerId, JsonElement StateData);

public sealed record IeltsSubmitRequestDto(string LearnerId, JsonElement StateData);
