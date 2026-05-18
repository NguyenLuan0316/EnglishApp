using WordWave.Application.Interfaces.Repositories;
using WordWave.Application.Services;
using WordWave.Application.Contracts.Patterns;
using WordWave.Domain.Models;
using Xunit;

namespace WordWave.Application.Tests.Services;

public class PatternServiceTests
{
    [Fact]
    public async Task GetAllAsync_Should_Map_Domain_To_Dto()
    {
        var repo = new FakePatternRepository
        {
            Data = new List<SentencePattern>
            {
                new()
                {
                    Id = 5,
                    Sentence = "How are you?",
                    Type = "greetings",
                    Meaning = "Ban khoe khong?",
                    Explanation = "greeting",
                    Examples = ["How are you today?"]
                }
            }
        };

        var service = new PatternService(repo);
        var result = await service.GetAllAsync(new PatternQuery(null, null));

        Assert.Single(result);
        Assert.Equal(5, result[0].Id);
        Assert.Equal("How are you?", result[0].Sentence);
        Assert.Equal("greetings", result[0].Type);
        Assert.Single(result[0].Examples);
    }

    private sealed class FakePatternRepository : IPatternRepository
    {
        public List<SentencePattern> Data { get; init; } = [];
        public Task<List<SentencePattern>> GetAllAsync(PatternQuery query) => Task.FromResult(Data);
        public Task<SentencePattern?> GetByIdAsync(int id) => Task.FromResult(Data.FirstOrDefault(x => x.Id == id));
    }
}
