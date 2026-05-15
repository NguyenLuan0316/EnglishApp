using WordWave.Application.Contracts.Toeic;

namespace WordWave.Application.Interfaces;

public interface IToeicDataSourceCrawler
{
    Task<ToeicRawData> CrawlAsync(string keyword, string sourceUrl, CancellationToken cancellationToken = default);
    Task<ToeicImportPackage> NormalizeAsync(ToeicRawData rawData, CancellationToken cancellationToken = default);
    Task<ToeicImportResultDto> SaveAsync(ToeicImportPackage normalizedData, CancellationToken cancellationToken = default);
}
