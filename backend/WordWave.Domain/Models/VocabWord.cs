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

[Table("word_progress")]
public class WordProgress
{
    [Column("word_id")]
    public int WordId { get; set; }

    [Column("correct_count")]
    public int CorrectCount { get; set; }

    [Column("wrong_count")]
    public int WrongCount { get; set; }

    [Column("next_review")]
    public DateTime NextReview { get; set; } = DateTime.UtcNow;

    [Column("last_reviewed")]
    public DateTime? LastReviewed { get; set; }
}

public class SubmitRequest
{
    public int WordId { get; set; }
    public bool Correct { get; set; }
}