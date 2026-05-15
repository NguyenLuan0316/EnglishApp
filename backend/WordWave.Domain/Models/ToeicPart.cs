using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WordWave.Domain.Models;

[Table("toeic_parts")]
public class ToeicPart
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("toeic_test_id")]
    public int ToeicTestId { get; set; }

    [Column("part_number")]
    public int PartNumber { get; set; }

    [Column("name")]
    public string Name { get; set; } = "";

    [Column("instructions")]
    public string Instructions { get; set; } = "";

    [Column("order_index")]
    public int OrderIndex { get; set; }

    public ToeicTest? Test { get; set; }
    public List<ToeicQuestion> Questions { get; set; } = [];
    public List<ToeicPassage> Passages { get; set; } = [];
    public List<ToeicAudio> Audios { get; set; } = [];
}
