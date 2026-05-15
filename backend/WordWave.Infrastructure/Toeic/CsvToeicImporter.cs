using System.Text;
using WordWave.Application.Contracts.Toeic;
using WordWave.Application.Interfaces;

namespace WordWave.Infrastructure.Toeic;

public class CsvToeicImporter : IToeicImporter
{
    public string SourceType => "csv";

    public async Task<ToeicImportPackage> ParseAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        var lines = new List<string>();
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(line))
            {
                lines.Add(line);
            }
        }

        if (lines.Count < 2)
        {
            throw new InvalidDataException("CSV file must contain a header and at least one data row.");
        }

        var headers = SplitCsvLine(lines[0]).Select(NormalizeHeader).ToList();
        var rows = lines.Skip(1).Select(line => ToRow(headers, SplitCsvLine(line))).ToList();

        var first = rows[0];
        var package = new ToeicImportPackage
        {
            Title = Get(first, "title"),
            Description = Get(first, "description"),
            SourceType = string.IsNullOrWhiteSpace(Get(first, "sourcetype")) ? SourceType : Get(first, "sourcetype"),
            SourceName = string.IsNullOrWhiteSpace(Get(first, "sourcename")) ? fileName : Get(first, "sourcename"),
            SourceUrl = Get(first, "sourceurl"),
            License = Get(first, "license"),
            ContentOwner = Get(first, "contentowner")
        };

        var parts = new Dictionary<int, ToeicImportPart>();
        var passages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var audios = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var questions = new Dictionary<string, ToeicImportQuestion>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            if (!int.TryParse(Get(row, "partnumber"), out var partNumber))
            {
                continue;
            }

            if (!parts.TryGetValue(partNumber, out var part))
            {
                part = new ToeicImportPart
                {
                    PartNumber = partNumber,
                    Name = Get(row, "partname"),
                    Instructions = Get(row, "instructions")
                };
                parts[partNumber] = part;
                package.Parts.Add(part);
            }

            var passageKey = Get(row, "passagekey");
            if (!string.IsNullOrWhiteSpace(passageKey) && passages.Add($"{partNumber}:{passageKey}"))
            {
                part.Passages.Add(new ToeicImportPassage
                {
                    Key = passageKey,
                    Title = Get(row, "passagetitle"),
                    Content = Get(row, "passagecontent")
                });
            }

            var audioKey = Get(row, "audiokey");
            if (!string.IsNullOrWhiteSpace(audioKey) && audios.Add($"{partNumber}:{audioKey}"))
            {
                part.Audios.Add(new ToeicImportAudio
                {
                    Key = audioKey,
                    Url = Get(row, "audiourl"),
                    LocalPath = Get(row, "audiolocalpath"),
                    Transcript = Get(row, "audiotranscript")
                });
            }

            if (!int.TryParse(Get(row, "questionnumber"), out var questionNumber))
            {
                continue;
            }

            var questionKey = $"{partNumber}:{questionNumber}";
            if (!questions.TryGetValue(questionKey, out var question))
            {
                question = new ToeicImportQuestion
                {
                    QuestionNumber = questionNumber,
                    Prompt = Get(row, "prompt"),
                    QuestionText = Get(row, "questiontext"),
                    ImageUrl = Get(row, "imageurl"),
                    Difficulty = Get(row, "difficulty"),
                    Explanation = Get(row, "explanation"),
                    PassageKey = passageKey,
                    AudioKey = audioKey
                };
                questions[questionKey] = question;
                part.Questions.Add(question);
            }

            var answerLabel = Get(row, "answerlabel");
            var answerText = Get(row, "answertext");
            if (!string.IsNullOrWhiteSpace(answerLabel) || !string.IsNullOrWhiteSpace(answerText))
            {
                question.Answers.Add(new ToeicImportAnswer
                {
                    Label = answerLabel,
                    AnswerText = answerText,
                    IsCorrect = bool.TryParse(Get(row, "iscorrect"), out var isCorrect) && isCorrect
                });
            }
        }

        return package;
    }

    private static Dictionary<string, string> ToRow(IReadOnlyList<string> headers, IReadOnlyList<string> values)
    {
        var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < headers.Count; i++)
        {
            row[headers[i]] = i < values.Count ? values[i] : "";
        }

        return row;
    }

    private static string Get(IReadOnlyDictionary<string, string> row, string key) => row.TryGetValue(key, out var value) ? value.Trim() : "";

    private static string NormalizeHeader(string header) => new(header.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());

    private static List<string> SplitCsvLine(string line)
    {
        var values = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"' && inQuotes && i + 1 < line.Length && line[i + 1] == '"')
            {
                current.Append('"');
                i++;
            }
            else if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        values.Add(current.ToString());
        return values;
    }
}
