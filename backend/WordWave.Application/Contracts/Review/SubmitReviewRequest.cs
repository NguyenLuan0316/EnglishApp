using System.ComponentModel.DataAnnotations;

namespace WordWave.Application.Contracts.Review;

public sealed record SubmitReviewRequest(
    [property: Range(1, int.MaxValue)] int WordId,
    bool Correct
);
