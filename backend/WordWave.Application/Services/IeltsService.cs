using System.Text.Json;
using System.Text.RegularExpressions;
using WordWave.Application.Contracts.Ielts;
using WordWave.Application.Interfaces;
using WordWave.Application.Interfaces.Repositories;
using WordWave.Domain.Models;

namespace WordWave.Application.Services;

public partial class IeltsService : IIeltsService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IIeltsRepository _repo;

    public IeltsService(IIeltsRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<IeltsTestSummaryDto>> GetTestsAsync(string learnerId)
    {
        learnerId = NormalizeLearnerId(learnerId);
        var tests = await _repo.GetTestsAsync();
        var attempts = (await _repo.GetAttemptsAsync(learnerId)).ToDictionary(x => x.IeltsTestId);

        return tests.Select(test =>
        {
            attempts.TryGetValue(test.Id, out var attempt);
            return new IeltsTestSummaryDto(
                test.Id,
                test.Title,
                test.Description,
                test.SourceType,
                test.SourceName,
                test.QuestionCount,
                test.CreatedAt,
                attempt is null ? null : MapAttemptSummary(attempt)
            );
        }).ToList();
    }

    public async Task<IeltsTestDetailDto?> GetTestByIdAsync(int id, string learnerId)
    {
        learnerId = NormalizeLearnerId(learnerId);
        var test = await _repo.GetTestByIdAsync(id);
        if (test is null) return null;

        var attempt = await _repo.GetAttemptAsync(id, learnerId);
        var data = DeserializeTest(test.TestData);

        return new IeltsTestDetailDto(
            test.Id,
            test.Title,
            test.Description,
            test.SourceType,
            test.SourceName,
            test.QuestionCount,
            test.CreatedAt,
            ToJsonElement(SanitizeTestData(data)),
            attempt is null ? null : MapAttempt(attempt)
        );
    }

    public async Task<IeltsAttemptDto?> GetAttemptAsync(int testId, string learnerId)
    {
        learnerId = NormalizeLearnerId(learnerId);
        var attempt = await _repo.GetAttemptAsync(testId, learnerId);
        return attempt is null ? null : MapAttempt(attempt);
    }

    public async Task<IeltsAttemptDto?> SaveAttemptAsync(int testId, IeltsSaveAttemptRequestDto request)
    {
        var test = await _repo.GetTestByIdAsync(testId);
        if (test is null) return null;

        var now = DateTime.UtcNow;
        var existing = await _repo.GetAttemptAsync(testId, NormalizeLearnerId(request.LearnerId));
        var attempt = new IeltsAttempt
        {
            Id = existing?.Id ?? 0,
            IeltsTestId = testId,
            LearnerId = NormalizeLearnerId(request.LearnerId),
            StateData = request.StateData.GetRawText(),
            ResultData = existing?.ResultData ?? "{}",
            IsSubmitted = existing?.IsSubmitted ?? false,
            OverallBand = existing?.OverallBand,
            CreatedAt = existing?.CreatedAt ?? now,
            UpdatedAt = now,
            SubmittedAt = existing?.SubmittedAt
        };

        return MapAttempt(await _repo.UpsertAttemptAsync(attempt));
    }

    public async Task<IeltsAttemptDto?> SubmitAsync(int testId, IeltsSubmitRequestDto request)
    {
        var test = await _repo.GetTestByIdAsync(testId);
        if (test is null) return null;

        var testData = DeserializeTest(test.TestData);
        var result = ScoreExam(testData, request.StateData);
        var now = DateTime.UtcNow;
        var existing = await _repo.GetAttemptAsync(testId, NormalizeLearnerId(request.LearnerId));
        var attempt = new IeltsAttempt
        {
            Id = existing?.Id ?? 0,
            IeltsTestId = testId,
            LearnerId = NormalizeLearnerId(request.LearnerId),
            StateData = request.StateData.GetRawText(),
            ResultData = JsonSerializer.Serialize(result, JsonOptions),
            IsSubmitted = true,
            OverallBand = result.OverallBand,
            CreatedAt = existing?.CreatedAt ?? now,
            UpdatedAt = now,
            SubmittedAt = now
        };

        return MapAttempt(await _repo.UpsertAttemptAsync(attempt));
    }

    private static IeltsTestData DeserializeTest(string json)
    {
        return JsonSerializer.Deserialize<IeltsTestData>(json, JsonOptions)
            ?? throw new InvalidOperationException("IELTS test data is invalid.");
    }

    private static object SanitizeTestData(IeltsTestData data)
    {
        return new
        {
            listening = new
            {
                parts = data.Listening.Parts.Select(SanitizePart)
            },
            reading = new
            {
                parts = data.Reading.Parts.Select(SanitizePart)
            },
            writing = new
            {
                tasks = data.Writing.Tasks.Select(task => new
                {
                    task.Id,
                    task.Title,
                    task.MinWords,
                    task.Minutes,
                    task.Prompt
                })
            },
            speaking = new
            {
                parts = data.Speaking.Parts.Select(part => new
                {
                    part.Id,
                    part.Title,
                    part.Time,
                    prompts = part.Prompts
                })
            }
        };
    }

    private static object SanitizePart(IeltsPartData part)
    {
        return new
        {
            part.Id,
            part.Title,
            part.Instruction,
            part.Transcript,
            part.Passage,
            questions = part.Questions.Select(question => new
            {
                question.Id,
                question.Number,
                question.Type,
                question.Prompt,
                question.Options
            })
        };
    }

    private static IeltsResultData ScoreExam(IeltsTestData test, JsonElement state)
    {
        var answers = ExtractAnswers(state);
        var listening = ScoreObjective(test.Listening.Parts.SelectMany(x => x.Questions), answers, true);
        var reading = ScoreObjective(test.Reading.Parts.SelectMany(x => x.Questions), answers, false);
        var writing = ScoreWriting(test.Writing.Tasks, answers);
        var speaking = ScoreSpeaking(test.Speaking, answers);
        var overallBand = RoundHalf((listening.Band + reading.Band + writing.Band + speaking.Band) / 4m);

        return new IeltsResultData(listening, reading, writing, speaking, overallBand);
    }

    private static Dictionary<string, string> ExtractAnswers(JsonElement state)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!state.TryGetProperty("answers", out var answers) || answers.ValueKind != JsonValueKind.Object)
        {
            return result;
        }

        foreach (var answer in answers.EnumerateObject())
        {
            result[answer.Name] = answer.Value.ValueKind == JsonValueKind.String
                ? answer.Value.GetString() ?? ""
                : answer.Value.ToString();
        }

        return result;
    }

    private static IeltsObjectiveResultData ScoreObjective(IEnumerable<IeltsQuestionData> questions, Dictionary<string, string> answers, bool listening)
    {
        var list = questions.ToList();
        var items = list.Select(question =>
        {
            answers.TryGetValue(question.Id, out var submitted);
            var isCorrect = MatchesAnswer(submitted ?? "", question.Answer);
            return new IeltsObjectiveItemResultData(question.Id, question.Number, submitted ?? "", question.Answer.FirstOrDefault() ?? "", isCorrect);
        }).ToList();
        var correct = items.Count(x => x.IsCorrect);
        var answered = items.Count(x => !string.IsNullOrWhiteSpace(x.Submitted));
        var band = listening ? ListeningBand(correct) : ReadingBand(correct);

        return new IeltsObjectiveResultData(list.Count, answered, correct, band, items);
    }

    private static IeltsWritingResultData ScoreWriting(List<IeltsWritingTaskData> tasks, Dictionary<string, string> answers)
    {
        var task1Config = tasks.FirstOrDefault(x => x.Id == "W1") ?? tasks[0];
        var task2Config = tasks.FirstOrDefault(x => x.Id == "W2") ?? tasks[^1];
        var task1 = ScoreProduction(answers.GetValueOrDefault(task1Config.Id, ""), task1Config.MinWords, task1Config.KeyTerms);
        var task2 = ScoreProduction(answers.GetValueOrDefault(task2Config.Id, ""), task2Config.MinWords, task2Config.KeyTerms);
        var band = RoundHalf((task1.Band + task2.Band * 2m) / 3m);
        return new IeltsWritingResultData(band, task1, task2);
    }

    private static IeltsProductionResultData ScoreSpeaking(IeltsSpeakingData speaking, Dictionary<string, string> answers)
    {
        var combined = string.Join(" ", speaking.Parts.Select(part => answers.GetValueOrDefault(part.Id, "")));
        return ScoreProduction(combined, 180, speaking.KeyTerms);
    }

    private static IeltsProductionResultData ScoreProduction(string text, int minWords, List<string> keyTerms)
    {
        var words = WordRegex().Matches(text.ToLowerInvariant()).Select(x => x.Value).ToList();
        if (words.Count == 0)
        {
            return new IeltsProductionResultData(
                0,
                0,
                new IeltsCriteriaResultData(0, 0, 0, 0),
                ["No response submitted."]
            );
        }

        var sentences = text.Split(['.', '!', '?'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var paragraphs = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var uniqueRatio = words.Distinct().Count() / (decimal)words.Count;
        var connectors = CountMatches(words, ["however", "therefore", "moreover", "although", "whereas", "because", "firstly", "secondly", "overall", "addition", "consequently"]);
        var topicTerms = CountMatches(words, keyTerms);
        var lengthRatio = Math.Min(words.Count / (decimal)minWords, 1.25m);

        var task = Clamp(4m + lengthRatio * 1.8m + Math.Min(topicTerms, 6) * 0.25m, 4m, 8.5m);
        var coherence = Clamp(4.2m + Math.Min(paragraphs.Length, 4) * 0.45m + Math.Min(connectors, 8) * 0.18m, 4m, 8.5m);
        var lexical = Clamp(4.1m + uniqueRatio * 4.2m + Math.Min(words.Count, 320) / 220m, 4m, 8.5m);
        var grammar = Clamp(4.2m + Math.Min(sentences.Length, 14) * 0.18m + AverageSentenceLength(words.Count, sentences.Length) / 20m, 4m, 8.5m);
        var band = RoundHalf((task + coherence + lexical + grammar) / 4m);

        if (words.Count < minWords * 0.5m) band = Math.Min(band, 5m);
        if (words.Count < minWords * 0.25m) band = Math.Min(band, 4m);

        var feedback = new List<string>();
        if (words.Count < minWords) feedback.Add($"Under the suggested minimum of {minWords} words.");
        if (paragraphs.Length < 2 && minWords > 200) feedback.Add("Use clearer paragraphing.");
        if (connectors < 3) feedback.Add("Add more linking phrases to improve coherence.");
        if (uniqueRatio < 0.42m) feedback.Add("Increase lexical range and avoid repeating the same wording.");
        if (feedback.Count == 0) feedback.Add("Response length, cohesion and vocabulary range are on track.");

        return new IeltsProductionResultData(
            band,
            words.Count,
            new IeltsCriteriaResultData(RoundHalf(task), RoundHalf(coherence), RoundHalf(lexical), RoundHalf(grammar)),
            feedback
        );
    }

    private static bool MatchesAnswer(string submitted, List<string> accepted)
    {
        if (string.IsNullOrWhiteSpace(submitted)) return false;
        var normalized = NormalizeAnswer(submitted);
        return accepted.Any(item => NormalizeAnswer(item) == normalized);
    }

    private static string NormalizeAnswer(string value)
    {
        return NonAnswerCharRegex()
            .Replace(value.ToLowerInvariant().Replace("&", " and ").Replace("%", " percent "), " ")
            .Replace("per cent", "percent")
            .Trim();
    }

    private static int CountMatches(List<string> words, IReadOnlyCollection<string> terms)
    {
        var text = string.Join(" ", words);
        return terms.Count(term => text.Contains(term, StringComparison.OrdinalIgnoreCase));
    }

    private static decimal AverageSentenceLength(int wordCount, int sentenceCount) =>
        sentenceCount == 0 ? Math.Min(wordCount, 18) : wordCount / (decimal)sentenceCount;

    private static decimal Clamp(decimal value, decimal min, decimal max) => Math.Min(Math.Max(value, min), max);

    private static decimal RoundHalf(decimal value) => Math.Round(value * 2m, MidpointRounding.AwayFromZero) / 2m;

    private static decimal ListeningBand(int raw) => BandFromMap(raw, [
        (39, 9m), (37, 8.5m), (35, 8m), (32, 7.5m), (30, 7m), (26, 6.5m),
        (23, 6m), (18, 5.5m), (16, 5m), (13, 4.5m), (10, 4m), (8, 3.5m),
        (6, 3m), (4, 2.5m), (1, 2m), (0, 0m)
    ]);

    private static decimal ReadingBand(int raw) => BandFromMap(raw, [
        (39, 9m), (37, 8.5m), (35, 8m), (33, 7.5m), (30, 7m), (27, 6.5m),
        (23, 6m), (19, 5.5m), (15, 5m), (13, 4.5m), (10, 4m), (8, 3.5m),
        (6, 3m), (4, 2.5m), (1, 2m), (0, 0m)
    ]);

    private static decimal BandFromMap(int raw, IEnumerable<(int Min, decimal Band)> map) =>
        map.First(x => raw >= x.Min).Band;

    private static IeltsAttemptSummaryDto MapAttemptSummary(IeltsAttempt attempt)
    {
        return new IeltsAttemptSummaryDto(
            attempt.Id,
            IsStarted(attempt.StateData),
            attempt.IsSubmitted,
            attempt.OverallBand,
            CountAnswered(attempt.StateData),
            attempt.UpdatedAt,
            attempt.SubmittedAt
        );
    }

    private static IeltsAttemptDto MapAttempt(IeltsAttempt attempt)
    {
        return new IeltsAttemptDto(
            attempt.Id,
            attempt.IeltsTestId,
            attempt.LearnerId,
            ParseJsonElement(attempt.StateData),
            ParseJsonElement(string.IsNullOrWhiteSpace(attempt.ResultData) ? "{}" : attempt.ResultData),
            attempt.IsSubmitted,
            attempt.OverallBand,
            attempt.CreatedAt,
            attempt.UpdatedAt,
            attempt.SubmittedAt
        );
    }

    private static bool IsStarted(string stateData)
    {
        var state = ParseJsonElement(stateData);
        return state.TryGetProperty("started", out var started) && started.ValueKind == JsonValueKind.True;
    }

    private static int CountAnswered(string stateData)
    {
        var state = ParseJsonElement(stateData);
        return ExtractAnswers(state).Count(x => !string.IsNullOrWhiteSpace(x.Value));
    }

    private static JsonElement ParseJsonElement(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) json = "{}";
        return JsonSerializer.Deserialize<JsonElement>(json, JsonOptions);
    }

    private static JsonElement ToJsonElement(object value) => JsonSerializer.SerializeToElement(value, JsonOptions);

    private static string NormalizeLearnerId(string learnerId) =>
        string.IsNullOrWhiteSpace(learnerId) ? "default" : learnerId.Trim();

    [GeneratedRegex(@"[a-z]+(?:'[a-z]+)?|[0-9]+")]
    private static partial Regex WordRegex();

    [GeneratedRegex(@"[^a-z0-9.]+")]
    private static partial Regex NonAnswerCharRegex();

    private sealed record IeltsTestData(IeltsQuestionSectionData Listening, IeltsQuestionSectionData Reading, IeltsWritingData Writing, IeltsSpeakingData Speaking);
    private sealed record IeltsQuestionSectionData(List<IeltsPartData> Parts);
    private sealed record IeltsWritingData(List<IeltsWritingTaskData> Tasks);
    private sealed record IeltsSpeakingData(List<string> KeyTerms, List<IeltsPartData> Parts);
    private sealed record IeltsPartData(string Id, string Title, string Instruction, string Transcript, List<string> Passage, List<IeltsQuestionData> Questions, string Time, List<string> Prompts);
    private sealed record IeltsQuestionData(string Id, int Number, string Type, string Prompt, List<string> Options, List<string> Answer);
    private sealed record IeltsWritingTaskData(string Id, string Title, int MinWords, int Minutes, List<string> KeyTerms, string Prompt);
    private sealed record IeltsResultData(IeltsObjectiveResultData Listening, IeltsObjectiveResultData Reading, IeltsWritingResultData Writing, IeltsProductionResultData Speaking, decimal OverallBand);
    private sealed record IeltsObjectiveResultData(int Total, int Answered, int Correct, decimal Band, IReadOnlyList<IeltsObjectiveItemResultData> Items);
    private sealed record IeltsObjectiveItemResultData(string Id, int Number, string Submitted, string CorrectAnswer, bool IsCorrect);
    private sealed record IeltsWritingResultData(decimal Band, IeltsProductionResultData Task1, IeltsProductionResultData Task2);
    private sealed record IeltsProductionResultData(decimal Band, int WordCount, IeltsCriteriaResultData Criteria, IReadOnlyList<string> Feedback);
    private sealed record IeltsCriteriaResultData(decimal Task, decimal Coherence, decimal Lexical, decimal Grammar);
}
