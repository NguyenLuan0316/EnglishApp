using WordWave.Application.Contracts.Paging;
using WordWave.Application.Contracts.Toeic;
using WordWave.Application.Interfaces;
using WordWave.Application.Interfaces.Repositories;
using WordWave.Domain.Models;

namespace WordWave.Application.Services;

public class ToeicImportService : IToeicImportService
{
    private readonly IReadOnlyDictionary<string, IToeicImporter> _importers;
    private readonly IToeicDataSourceCrawler _crawler;
    private readonly IToeicRepository _repo;

    public ToeicImportService(IEnumerable<IToeicImporter> importers, IToeicDataSourceCrawler crawler, IToeicRepository repo)
    {
        _importers = importers.ToDictionary(x => x.SourceType, StringComparer.OrdinalIgnoreCase);
        _crawler = crawler;
        _repo = repo;
    }

    public Task<ToeicImportResultDto> ImportJsonAsync(Stream stream, string fileName) => ImportAsync("json", stream, fileName);

    public Task<ToeicImportResultDto> ImportCsvAsync(Stream stream, string fileName) => ImportAsync("csv", stream, fileName);

    public async Task<ToeicImportResultDto> CrawlAsync(string keyword, string sourceUrl)
    {
        var raw = await _crawler.CrawlAsync(keyword, sourceUrl);
        var normalized = await _crawler.NormalizeAsync(raw);
        return await _crawler.SaveAsync(normalized);
    }

    public async Task<PagedResult<ToeicImportLogDto>> GetImportLogsAsync(int page, int limit)
    {
        var total = await _repo.CountImportLogsAsync();
        var logs = await _repo.GetImportLogsAsync(page, limit);
        var data = logs.Select(x => new ToeicImportLogDto(
            x.Id,
            x.SourceType,
            x.SourceName,
            x.SourceUrl,
            x.Status,
            x.TotalItems,
            x.ImportedItems,
            x.FailedItems,
            x.ErrorMessage,
            x.Details,
            x.CreatedAt
        )).ToList();

        return new PagedResult<ToeicImportLogDto>(total, page, limit, data);
    }

    private async Task<ToeicImportResultDto> ImportAsync(string sourceType, Stream stream, string fileName)
    {
        if (!_importers.TryGetValue(sourceType, out var importer))
        {
            throw new InvalidOperationException($"No TOEIC importer registered for '{sourceType}'.");
        }

        var package = await importer.ParseAsync(stream, fileName);
        var result = await _crawler.SaveAsync(package);
        return result;
    }
}

public class ToeicImportPackageWriter : IToeicImportPackageWriter
{
    private static readonly HashSet<int> ValidParts = [1, 2, 3, 4, 5, 6, 7];
    private readonly IToeicRepository _repo;

    public ToeicImportPackageWriter(IToeicRepository repo) => _repo = repo;

    public async Task<ToeicImportResultDto> SaveAsync(ToeicImportPackage package, CancellationToken cancellationToken = default)
    {
        var errors = ValidatePackage(package);
        var totalItems = package.Parts.Sum(p => p.Questions.Count);

        if (errors.Count > 0 && totalItems == 0)
        {
            await WriteLogAsync(package, null, "failed", totalItems, 0, totalItems, errors);
            return new ToeicImportResultDto(null, "failed", totalItems, 0, totalItems, errors);
        }

        var test = new ToeicTest
        {
            Title = package.Title.Trim(),
            Description = package.Description.Trim(),
            SourceType = package.SourceType.Trim(),
            SourceName = package.SourceName.Trim(),
            SourceUrl = package.SourceUrl.Trim(),
            License = package.License.Trim(),
            ContentOwner = package.ContentOwner.Trim(),
            IsPublic = true,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var partInput in package.Parts.Where(p => ValidParts.Contains(p.PartNumber)).OrderBy(p => p.PartNumber))
        {
            var part = new ToeicPart
            {
                PartNumber = partInput.PartNumber,
                Name = string.IsNullOrWhiteSpace(partInput.Name) ? GetPartName(partInput.PartNumber) : partInput.Name.Trim(),
                Instructions = partInput.Instructions.Trim(),
                OrderIndex = partInput.PartNumber
            };

            var passagesByKey = new Dictionary<string, ToeicPassage>(StringComparer.OrdinalIgnoreCase);
            foreach (var passageInput in partInput.Passages)
            {
                var passage = new ToeicPassage
                {
                    Title = passageInput.Title.Trim(),
                    Content = passageInput.Content.Trim()
                };
                part.Passages.Add(passage);
                if (!string.IsNullOrWhiteSpace(passageInput.Key))
                {
                    passagesByKey[passageInput.Key.Trim()] = passage;
                }
            }

            var audiosByKey = new Dictionary<string, ToeicAudio>(StringComparer.OrdinalIgnoreCase);
            foreach (var audioInput in partInput.Audios)
            {
                var audio = new ToeicAudio
                {
                    Url = audioInput.Url.Trim(),
                    LocalPath = audioInput.LocalPath.Trim(),
                    Transcript = audioInput.Transcript.Trim()
                };
                part.Audios.Add(audio);
                if (!string.IsNullOrWhiteSpace(audioInput.Key))
                {
                    audiosByKey[audioInput.Key.Trim()] = audio;
                }
            }

            foreach (var questionInput in partInput.Questions)
            {
                var questionErrors = ValidateQuestion(partInput.PartNumber, questionInput);
                if (questionErrors.Count > 0)
                {
                    errors.AddRange(questionErrors);
                    continue;
                }

                var question = new ToeicQuestion
                {
                    QuestionNumber = questionInput.QuestionNumber,
                    Prompt = questionInput.Prompt.Trim(),
                    QuestionText = questionInput.QuestionText.Trim(),
                    ImageUrl = questionInput.ImageUrl.Trim(),
                    Difficulty = questionInput.Difficulty.Trim(),
                    Explanation = questionInput.Explanation.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                if (!string.IsNullOrWhiteSpace(questionInput.PassageKey) && passagesByKey.TryGetValue(questionInput.PassageKey.Trim(), out var passage))
                {
                    question.Passage = passage;
                }

                if (!string.IsNullOrWhiteSpace(questionInput.AudioKey) && audiosByKey.TryGetValue(questionInput.AudioKey.Trim(), out var audio))
                {
                    question.Audio = audio;
                }

                foreach (var answerInput in questionInput.Answers)
                {
                    question.Answers.Add(new ToeicAnswer
                    {
                        Label = answerInput.Label.Trim(),
                        AnswerText = answerInput.AnswerText.Trim(),
                        IsCorrect = answerInput.IsCorrect
                    });
                }

                part.Questions.Add(question);
            }

            if (part.Questions.Count > 0 || part.Passages.Count > 0 || part.Audios.Count > 0)
            {
                test.Parts.Add(part);
            }
        }

        var importedItems = test.Parts.Sum(p => p.Questions.Count);
        var failedItems = Math.Max(0, totalItems - importedItems);
        if (importedItems == 0)
        {
            errors.Add("No valid TOEIC questions were found.");
            await WriteLogAsync(package, null, "failed", totalItems, importedItems, failedItems, errors);
            return new ToeicImportResultDto(null, "failed", totalItems, importedItems, failedItems, errors);
        }

        var testId = await _repo.AddTestAsync(test);
        var status = failedItems == 0 && errors.Count == 0 ? "success" : "partial";
        await WriteLogAsync(package, testId, status, totalItems, importedItems, failedItems, errors);

        return new ToeicImportResultDto(testId, status, totalItems, importedItems, failedItems, errors);
    }

    private static List<string> ValidatePackage(ToeicImportPackage package)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(package.Title)) errors.Add("Test title is required.");
        if (string.IsNullOrWhiteSpace(package.SourceType)) errors.Add("Source type is required.");
        if (string.IsNullOrWhiteSpace(package.SourceName)) errors.Add("Source name is required.");
        if (string.IsNullOrWhiteSpace(package.License)) errors.Add("License or permission metadata is required.");
        if (string.IsNullOrWhiteSpace(package.ContentOwner)) errors.Add("Content owner is required.");
        if (package.Parts.Count == 0) errors.Add("At least one TOEIC part is required.");
        foreach (var part in package.Parts.Where(p => !ValidParts.Contains(p.PartNumber)))
        {
            errors.Add($"Part {part.PartNumber} is not valid. TOEIC parts must be 1 to 7.");
        }

        return errors;
    }

    private static List<string> ValidateQuestion(int partNumber, ToeicImportQuestion question)
    {
        var errors = new List<string>();
        var prefix = $"Part {partNumber}, question {question.QuestionNumber}:";
        if (question.QuestionNumber <= 0) errors.Add($"{prefix} questionNumber must be greater than 0.");
        if (string.IsNullOrWhiteSpace(question.QuestionText) && string.IsNullOrWhiteSpace(question.Prompt)) errors.Add($"{prefix} question text or prompt is required.");
        if (question.Answers.Count < 2) errors.Add($"{prefix} at least two answers are required.");
        if (question.Answers.Count(a => a.IsCorrect) != 1) errors.Add($"{prefix} exactly one correct answer is required.");
        if (question.Answers.Any(a => string.IsNullOrWhiteSpace(a.Label) || string.IsNullOrWhiteSpace(a.AnswerText))) errors.Add($"{prefix} answer label and text are required.");
        return errors;
    }

    private async Task WriteLogAsync(ToeicImportPackage package, int? testId, string status, int totalItems, int importedItems, int failedItems, IReadOnlyList<string> errors)
    {
        await _repo.AddImportLogAsync(new ToeicImportLog
        {
            SourceType = package.SourceType,
            SourceName = package.SourceName,
            SourceUrl = package.SourceUrl,
            Status = status,
            TotalItems = totalItems,
            ImportedItems = importedItems,
            FailedItems = failedItems,
            ErrorMessage = errors.FirstOrDefault() ?? "",
            Details = string.Join(Environment.NewLine, errors.Prepend(testId.HasValue ? $"testId={testId.Value}" : "testId=")),
            CreatedAt = DateTime.UtcNow
        });
    }

    private static string GetPartName(int partNumber) => partNumber switch
    {
        1 => "Picture Description",
        2 => "Question Response",
        3 => "Conversations",
        4 => "Talks",
        5 => "Incomplete Sentences",
        6 => "Text Completion",
        7 => "Reading Comprehension",
        _ => "Unknown"
    };
}
