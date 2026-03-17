// wordwave/backend/WordWave.Api/Models/Models.cs
namespace WordWave.Api.Models;

public class VocabWord
{
    public int    Id             { get; set; }
    public string Word           { get; set; } = "";
    public string Phonetic       { get; set; } = "";
    public string Meaning        { get; set; } = "";
    public string Example        { get; set; } = "";
    public string ExampleMeaning { get; set; } = "";
    public string Level          { get; set; } = "";
    public string Topic          { get; set; } = "";
}

public class GrammarLesson
{
    public int            Id          { get; set; }
    public string         Title       { get; set; } = "";
    public string         Level       { get; set; } = "";
    public string         Description { get; set; } = "";
    public string         Formula     { get; set; } = "";
    public List<Example>  Examples    { get; set; } = [];
    public string         Tips        { get; set; } = "";
}

public class Example
{
    public string En { get; set; } = "";
    public string Vi { get; set; } = "";
}

public class SentencePattern
{
    public int           Id          { get; set; }
    public string        Sentence    { get; set; } = "";
    public string        Meaning     { get; set; } = "";
    public string        Explanation { get; set; } = "";
    public List<string>  Examples    { get; set; } = [];
}

public class WordProgress
{
    public int      WordId       { get; set; }
    public int      CorrectCount { get; set; }
    public int      WrongCount   { get; set; }
    public DateTime NextReview   { get; set; } = DateTime.UtcNow;
    public DateTime? LastReviewed { get; set; }
}

public class SubmitRequest
{
    public int  WordId  { get; set; }
    public bool Correct { get; set; }
}
