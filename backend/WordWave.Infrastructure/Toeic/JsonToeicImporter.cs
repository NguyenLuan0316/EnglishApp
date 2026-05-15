using System.Text.Json;
using WordWave.Application.Contracts.Toeic;
using WordWave.Application.Interfaces;

namespace WordWave.Infrastructure.Toeic;

public class JsonToeicImporter : IToeicImporter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public virtual string SourceType => "json";

    public virtual async Task<ToeicImportPackage> ParseAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        var package = await JsonSerializer.DeserializeAsync<ToeicImportPackage>(stream, JsonOptions, cancellationToken);
        if (package is null)
        {
            throw new InvalidDataException("JSON file does not match the TOEIC import schema.");
        }

        if (string.IsNullOrWhiteSpace(package.SourceType))
        {
            package.SourceType = SourceType;
        }

        if (string.IsNullOrWhiteSpace(package.SourceName))
        {
            package.SourceName = fileName;
        }

        return package;
    }
}
