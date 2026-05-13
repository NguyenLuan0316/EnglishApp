namespace WordWave.Application.Contracts.Review;

public sealed record ReviewWordProgressDto(
    int WordId,
    int CorrectCount,
    int WrongCount,
    DateTime NextReview,
    DateTime? LastReviewed
);
