namespace WordWave.Application.Contracts.Review;

public sealed record ReviewProgressDto(int Total, int Learned, IReadOnlyDictionary<string, LevelProgressDto> ByLevel);
