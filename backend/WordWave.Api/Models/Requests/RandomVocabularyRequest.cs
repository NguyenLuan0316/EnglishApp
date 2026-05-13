using System.ComponentModel.DataAnnotations;

namespace WordWave.Api.Models.Requests;

public sealed class RandomVocabularyRequest
{
    public string? Level { get; init; }
    public string? Topic { get; init; }

    [Range(1, 100)]
    public int Count { get; init; } = 10;
}
