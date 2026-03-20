namespace WordWave.Domain.Models;
public class VocabWord
{
    public int Id { get; set; }
    public string Word { get; set; } = "";
    public string Phonetic { get; set; } = "";
    public string Meaning { get; set; } = "";
    public string Example { get; set; } = "";
    public string ExampleMeaning { get; set; } = "";
    public string Level { get; set; } = "";
    public string Topic { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class SentencePattern
{
    public int Id { get; set; }
    public string Sentence { get; set; } = "";
    public string Meaning { get; set; } = "";
    public string Explanation { get; set; } = "";
    public string[] Examples { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class GrammarLesson
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Level { get; set; } = "";
    public string Description { get; set; } = "";
    public string Formula { get; set; } = "";
    public string Tips { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<GrammarExample> GrammarExamples { get; set; } = [];
}

public class GrammarExample
{
    public int Id { get; set; }
    public int LessonId { get; set; }
    public string En { get; set; } = "";
    public string Vi { get; set; } = "";
}

public class WordProgress
{
    public int WordId { get; set; }
    public int CorrectCount { get; set; }
    public int WrongCount { get; set; }
    public DateTime NextReview { get; set; } = DateTime.UtcNow;
    public DateTime? LastReviewed { get; set; }
}

public class SubmitRequest
{
    public int WordId { get; set; }
    public bool Correct { get; set; }
}