using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WordWave.Domain.Models;

[Table("toeic_answers")]
public class ToeicAnswer
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("toeic_question_id")]
    public int ToeicQuestionId { get; set; }

    [Column("label")]
    public string Label { get; set; } = "";

    [Column("answer_text")]
    public string AnswerText { get; set; } = "";

    [Column("is_correct")]
    public bool IsCorrect { get; set; }

    public ToeicQuestion? Question { get; set; }
}
