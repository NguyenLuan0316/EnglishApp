using System.ComponentModel.DataAnnotations.Schema;

namespace WordWave.Domain.Models;

[Table("word_progress")]
public class WordProgress
{
    [Column("word_id")]
    public int WordId { get; set; }

    [Column("correct_count")]
    public int CorrectCount { get; set; }

    [Column("wrong_count")]
    public int WrongCount { get; set; }

    [Column("next_review")]
    public DateTime NextReview { get; set; } = DateTime.UtcNow;

    [Column("last_reviewed")]
    public DateTime? LastReviewed { get; set; }
}
