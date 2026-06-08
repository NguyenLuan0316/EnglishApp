using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WordWave.Domain.Models;

[Table("ielts_attempts")]
public class IeltsAttempt
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("ielts_test_id")]
    public int IeltsTestId { get; set; }

    [Column("learner_id")]
    public string LearnerId { get; set; } = "default";

    [Column("state_data")]
    public string StateData { get; set; } = "{}";

    [Column("result_data")]
    public string ResultData { get; set; } = "{}";

    [Column("is_submitted")]
    public bool IsSubmitted { get; set; }

    [Column("overall_band")]
    public decimal? OverallBand { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("submitted_at")]
    public DateTime? SubmittedAt { get; set; }

    public IeltsTest? Test { get; set; }
}
