using WordWave.Application.Contracts.Toeic;

namespace WordWave.Application.Interfaces;

public interface IToeicService
{
    Task<IReadOnlyList<ToeicTestSummaryDto>> GetTestsAsync();
    Task<ToeicTestDetailDto?> GetTestByIdAsync(int id);
    Task<IReadOnlyList<ToeicQuestionDto>> GetQuestionsAsync(int? part);
    Task<ToeicSubmitResultDto?> SubmitAsync(int testId, ToeicSubmitRequestDto request);
}
