using WordWave.Domain.Models;

namespace WordWave.Application.Interfaces;

public interface IPasswordService
{
    string Hash(AppUser user, string password);
    bool Verify(AppUser user, string password);
}
