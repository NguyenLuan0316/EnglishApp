using WordWave.Application.Contracts.Toeic;

namespace WordWave.Application.Interfaces;

public interface IToeicImporter
{
    string SourceType { get; }
    Task<ToeicImportPackage> ParseAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);
}
