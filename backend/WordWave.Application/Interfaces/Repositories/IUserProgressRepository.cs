using WordWave.Domain.Models;

namespace WordWave.Application.Interfaces.Repositories;

public interface IUserProgressRepository
{
    Task<List<UserWordProgress>> GetAllAsync(Guid userId);
    Task<UserWordProgress> RecordAsync(Guid userId, int wordId, bool correct);
}
