using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using WordWave.Application.Interfaces;
using WordWave.Domain.Models;

namespace WordWave.Infrastructure.Security;

public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<AppUser> _hasher = new(Options.Create(new PasswordHasherOptions { IterationCount = 210_000 }));
    public string Hash(AppUser user, string password) => _hasher.HashPassword(user, password);
    public bool Verify(AppUser user, string password) => _hasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Failed;
}
