using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WordWave.Domain.Models;

[Table("sentence_patterns")]
public class SentencePattern
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("sentence")]
    public string Sentence { get; set; } = "";

    [Column("meaning")]
    public string Meaning { get; set; } = "";

    [Column("explanation")]
    public string Explanation { get; set; } = "";

    [Column("examples")]
    public string[] Examples { get; set; } = [];

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
