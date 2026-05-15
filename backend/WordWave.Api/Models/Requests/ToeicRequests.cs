using System.ComponentModel.DataAnnotations;
using WordWave.Application.Contracts.Toeic;

namespace WordWave.Api.Models.Requests;

public sealed class ToeicCrawlRequest
{
    [Required]
    [MaxLength(100)]
    public string Keyword { get; init; } = "";

    [Required]
    [Url]
    public string SourceUrl { get; init; } = "";
}

public sealed class ToeicQuestionQueryRequest
{
    [Range(1, 7)]
    public int? Part { get; init; }
}

public sealed class ToeicImportLogQueryRequest
{
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 200)]
    public int Limit { get; init; } = 50;
}

public sealed class ToeicSubmitRequest
{
    [Required]
    public List<ToeicSubmitAnswerDto> Answers { get; init; } = [];
}
