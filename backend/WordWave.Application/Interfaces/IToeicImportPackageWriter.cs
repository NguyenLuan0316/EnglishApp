using WordWave.Application.Contracts.Toeic;

namespace WordWave.Application.Interfaces;

public interface IToeicImportPackageWriter
{
    Task<ToeicImportResultDto> SaveAsync(ToeicImportPackage package, CancellationToken cancellationToken = default);
}
