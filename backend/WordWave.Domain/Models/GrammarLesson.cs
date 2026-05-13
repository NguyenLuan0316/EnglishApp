using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WordWave.Domain.Models;

[Table("grammar_lessons")]
public class GrammarLesson
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("title")]
    public string Title { get; set; } = "";

    [Column("level")]
    public string Level { get; set; } = "";

    [Column("description")]
    public string Description { get; set; } = "";

    [Column("formula")]
    public string Formula { get; set; } = "";

    [Column("tips")]
    public string Tips { get; set; } = "";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<GrammarExample> GrammarExamples { get; set; } = [];
}
