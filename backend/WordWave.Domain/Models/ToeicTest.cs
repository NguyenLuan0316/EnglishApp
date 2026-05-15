using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WordWave.Domain.Models;

[Table("toeic_tests")]
public class ToeicTest
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("title")]
    public string Title { get; set; } = "";

    [Column("description")]
    public string Description { get; set; } = "";

    [Column("source_type")]
    public string SourceType { get; set; } = "manual";

    [Column("source_name")]
    public string SourceName { get; set; } = "";

    [Column("source_url")]
    public string SourceUrl { get; set; } = "";

    [Column("license")]
    public string License { get; set; } = "";

    [Column("content_owner")]
    public string ContentOwner { get; set; } = "";

    [Column("is_public")]
    public bool IsPublic { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<ToeicPart> Parts { get; set; } = [];
}
