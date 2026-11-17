using System.ComponentModel.DataAnnotations;

namespace WordWave.Application.Contracts.Review;

public sealed record SubmitLearningRequest([RegularExpression("^(vocabulary|grammar|patterns)$")] string ContentType, [Range(1, int.MaxValue)] int ContentId, bool Correct, bool Daily = false);
