using WordWave.Application.Contracts.Toeic;
using WordWave.Application.Interfaces;
using WordWave.Application.Interfaces.Repositories;
using WordWave.Domain.Models;

namespace WordWave.Application.Services;

public class ToeicService : IToeicService
{
    private readonly IToeicRepository _repo;

    public ToeicService(IToeicRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<ToeicTestSummaryDto>> GetTestsAsync()
    {
        var tests = await _repo.GetTestsAsync();
        return tests.Select(t => new ToeicTestSummaryDto(
            t.Id,
            t.Title,
            t.Description,
            t.SourceType,
            t.SourceName,
            t.Parts.Count,
            t.Parts.Sum(p => p.Questions.Count)
        )).ToList();
    }

    public async Task<ToeicTestDetailDto?> GetTestByIdAsync(int id)
    {
        var test = await _repo.GetTestByIdAsync(id);
        return test is null ? null : MapTest(test);
    }

    public async Task<IReadOnlyList<ToeicQuestionDto>> GetQuestionsAsync(int? part)
    {
        var questions = await _repo.GetQuestionsAsync(part);
        return questions.Select(MapQuestion).ToList();
    }

    public async Task<ToeicSubmitResultDto?> SubmitAsync(int testId, ToeicSubmitRequestDto request)
    {
        var questions = await _repo.GetQuestionsByTestIdAsync(testId);
        if (questions.Count == 0)
        {
            return null;
        }

        var submitted = request.Answers
            .GroupBy(x => x.QuestionId)
            .ToDictionary(x => x.Key, x => x.Last().AnswerId);

        var results = questions.Select(q =>
        {
            submitted.TryGetValue(q.Id, out var selectedAnswerId);
            var correctAnswer = q.Answers.FirstOrDefault(a => a.IsCorrect);
            var isCorrect = correctAnswer is not null && selectedAnswerId == correctAnswer.Id;

            return new ToeicSubmitItemResultDto(
                q.Id,
                selectedAnswerId == 0 ? null : selectedAnswerId,
                isCorrect,
                correctAnswer?.Id,
                q.Explanation
            );
        }).ToList();

        var correct = results.Count(x => x.IsCorrect);
        var answered = results.Count(x => x.SelectedAnswerId.HasValue);
        var score = questions.Count == 0 ? 0 : decimal.Round(correct * 100m / questions.Count, 2);

        return new ToeicSubmitResultDto(testId, questions.Count, answered, correct, score, results);
    }

    private static ToeicTestDetailDto MapTest(ToeicTest test)
    {
        var parts = test.Parts
            .OrderBy(p => p.OrderIndex)
            .ThenBy(p => p.PartNumber)
            .Select(p => new ToeicPartDto(
                p.Id,
                p.PartNumber,
                p.Name,
                p.Instructions,
                p.Passages.OrderBy(x => x.Id).Select(MapPassage).ToList(),
                p.Audios.OrderBy(x => x.Id).Select(MapAudio).ToList(),
                p.Questions.OrderBy(x => x.QuestionNumber).Select(MapQuestion).ToList()
            ))
            .ToList();

        return new ToeicTestDetailDto(test.Id, test.Title, test.Description, test.SourceType, test.SourceName, test.License, parts);
    }

    private static ToeicQuestionDto MapQuestion(ToeicQuestion question)
    {
        return new ToeicQuestionDto(
            question.Id,
            question.Part?.PartNumber ?? 0,
            question.QuestionNumber,
            question.Prompt,
            question.QuestionText,
            question.ImageUrl,
            question.Difficulty,
            question.Passage is null ? null : MapPassage(question.Passage),
            question.Audio is null ? null : MapAudio(question.Audio),
            question.Answers.OrderBy(a => a.Label).Select(a => new ToeicAnswerOptionDto(a.Id, a.Label, a.AnswerText)).ToList()
        );
    }

    private static ToeicPassageDto MapPassage(ToeicPassage passage) => new(passage.Id, passage.Title, passage.Content);

    private static ToeicAudioDto MapAudio(ToeicAudio audio) => new(audio.Id, audio.Url, audio.LocalPath, audio.Transcript);
}
