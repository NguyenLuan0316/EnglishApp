namespace WordWave.Application.Contracts.Patterns;

public sealed record PatternQuery(
    string? Search,
    string? Purpose
);
