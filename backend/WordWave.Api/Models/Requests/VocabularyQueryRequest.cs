using System.ComponentModel.DataAnnotations;

namespace WordWave.Api.Models.Requests;

public sealed class VocabularyQueryRequest
{
    public string? Level { get; init; }
    public string? Topic { get; init; }
    public string? Search { get; init; }

    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 200)]
    public int Limit { get; init; } = 20;
}
