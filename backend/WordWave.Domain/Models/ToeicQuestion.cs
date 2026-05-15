using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WordWave.Domain.Models;

[Table("toeic_questions")]
public class ToeicQuestion
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("toeic_part_id")]
    public int ToeicPartId { get; set; }

    [Column("toeic_passage_id")]
    public int? ToeicPassageId { get; set; }

    [Column("toeic_audio_id")]
    public int? ToeicAudioId { get; set; }

    [Column("question_number")]
    public int QuestionNumber { get; set; }

    [Column("prompt")]
    public string Prompt { get; set; } = "";

    [Column("question_text")]
    public string QuestionText { get; set; } = "";

    [Column("image_url")]
    public string ImageUrl { get; set; } = "";

    [Column("difficulty")]
    public string Difficulty { get; set; } = "";

    [Column("explanation")]
    public string Explanation { get; set; } = "";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ToeicPart? Part { get; set; }
    public ToeicPassage? Passage { get; set; }
    public ToeicAudio? Audio { get; set; }
    public List<ToeicAnswer> Answers { get; set; } = [];
}
