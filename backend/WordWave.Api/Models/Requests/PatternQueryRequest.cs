namespace WordWave.Api.Models.Requests;

public sealed class PatternQueryRequest
{
    public string? Search { get; init; }
    public string? Purpose { get; init; }
    public string? Type { get; init; }
}
