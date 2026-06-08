using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WordWave.Domain.Models;

[Table("ielts_tests")]
public class IeltsTest
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("title")]
    public string Title { get; set; } = "";

    [Column("description")]
    public string Description { get; set; } = "";

    [Column("source_type")]
    public string SourceType { get; set; } = "seed";

    [Column("source_name")]
    public string SourceName { get; set; } = "WordWave";

    [Column("test_data")]
    public string TestData { get; set; } = "{}";

    [Column("question_count")]
    public int QuestionCount { get; set; }

    [Column("is_public")]
    public bool IsPublic { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<IeltsAttempt> Attempts { get; set; } = [];
}
