using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WordWave.Domain.Models;

[Table("toeic_passages")]
public class ToeicPassage
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("toeic_part_id")]
    public int ToeicPartId { get; set; }

    [Column("title")]
    public string Title { get; set; } = "";

    [Column("content")]
    public string Content { get; set; } = "";

    public ToeicPart? Part { get; set; }
    public List<ToeicQuestion> Questions { get; set; } = [];
}
