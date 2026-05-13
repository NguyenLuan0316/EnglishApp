using WordWave.Application.Interfaces.Repositories;
using WordWave.Application.Services;
using WordWave.Domain.Models;
using Xunit;

namespace WordWave.Application.Tests.Services;

public class GrammarServiceTests
{
    [Fact]
    public async Task GetByIdAsync_Should_Map_Examples()
    {
        var repo = new FakeGrammarRepository
        {
            Data = new List<GrammarLesson>
            {
                new()
                {
                    Id = 10,
                    Title = "Present Simple",
                    Level = "A1",
                    Description = "desc",
                    Formula = "S + V",
                    Tips = "tip",
                    GrammarExamples =
                    [
                        new GrammarExample { En = "I go to school.", Vi = "Toi di hoc." }
                    ]
                }
            }
        };

        var service = new GrammarService(repo);
        var dto = await service.GetByIdAsync(10);

        Assert.NotNull(dto);
        Assert.Equal("Present Simple", dto!.Title);
        Assert.Single(dto.Examples);
        Assert.Equal("I go to school.", dto.Examples[0].En);
    }

    private sealed class FakeGrammarRepository : IGrammarRepository
    {
        public List<GrammarLesson> Data { get; init; } = [];
        public Task<List<GrammarLesson>> GetAllAsync() => Task.FromResult(Data);
        public Task<GrammarLesson?> GetByIdAsync(int id) => Task.FromResult(Data.FirstOrDefault(x => x.Id == id));
    }
}
