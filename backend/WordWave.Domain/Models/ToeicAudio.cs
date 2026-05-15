using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WordWave.Domain.Models;

[Table("toeic_audios")]
public class ToeicAudio
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("toeic_part_id")]
    public int ToeicPartId { get; set; }

    [Column("url")]
    public string Url { get; set; } = "";

    [Column("local_path")]
    public string LocalPath { get; set; } = "";

    [Column("transcript")]
    public string Transcript { get; set; } = "";

    public ToeicPart? Part { get; set; }
    public List<ToeicQuestion> Questions { get; set; } = [];
}
