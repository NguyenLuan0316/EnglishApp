using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WordWave.Domain.Models;

[Table("grammar_examples")]
public class GrammarExample
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("lesson_id")]
    public int LessonId { get; set; }

    [Column("en")]
    public string En { get; set; } = "";

    [Column("vi")]
    public string Vi { get; set; } = "";
}
