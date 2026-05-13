using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WordWave.Domain.Models;

[Table("vocabulary")]
public class VocabWord
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("word")]
    public string Word { get; set; } = "";

    [Column("phonetic")]
    public string Phonetic { get; set; } = "";

    [Column("meaning")]
    public string Meaning { get; set; } = "";

    [Column("example")]
    public string Example { get; set; } = "";

    [Column("example_meaning")]
    public string ExampleMeaning { get; set; } = "";

    [Column("level")]
    public string Level { get; set; } = "";

    [Column("topic")]
    public string Topic { get; set; } = "";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
