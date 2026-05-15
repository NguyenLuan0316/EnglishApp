using WordWave.Application.Contracts.Paging;
using WordWave.Application.Contracts.Toeic;

namespace WordWave.Application.Interfaces;

public interface IToeicImportService
{
    Task<ToeicImportResultDto> ImportJsonAsync(Stream stream, string fileName);
    Task<ToeicImportResultDto> ImportCsvAsync(Stream stream, string fileName);
    Task<ToeicImportResultDto> CrawlAsync(string keyword, string sourceUrl);
    Task<PagedResult<ToeicImportLogDto>> GetImportLogsAsync(int page, int limit);
}
