namespace WordWave.Application.Contracts.Toeic;

public sealed class ToeicImportPackage
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string SourceType { get; set; } = "";
    public string SourceName { get; set; } = "";
    public string SourceUrl { get; set; } = "";
    public string License { get; set; } = "";
    public string ContentOwner { get; set; } = "";
    public List<ToeicImportPart> Parts { get; set; } = [];
}

public sealed class ToeicImportPart
{
    public int PartNumber { get; set; }
    public string Name { get; set; } = "";
    public string Instructions { get; set; } = "";
    public List<ToeicImportPassage> Passages { get; set; } = [];
    public List<ToeicImportAudio> Audios { get; set; } = [];
    public List<ToeicImportQuestion> Questions { get; set; } = [];
}

public sealed class ToeicImportPassage
{
    public string Key { get; set; } = "";
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
}

public sealed class ToeicImportAudio
{
    public string Key { get; set; } = "";
    public string Url { get; set; } = "";
    public string LocalPath { get; set; } = "";
    public string Transcript { get; set; } = "";
}

public sealed class ToeicImportQuestion
{
    public int QuestionNumber { get; set; }
    public string Prompt { get; set; } = "";
    public string QuestionText { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public string Difficulty { get; set; } = "";
    public string Explanation { get; set; } = "";
    public string PassageKey { get; set; } = "";
    public string AudioKey { get; set; } = "";
    public List<ToeicImportAnswer> Answers { get; set; } = [];
}

public sealed class ToeicImportAnswer
{
    public string Label { get; set; } = "";
    public string AnswerText { get; set; } = "";
    public bool IsCorrect { get; set; }
}

public sealed class ToeicRawData
{
    public string SourceType { get; set; } = "";
    public string SourceName { get; set; } = "";
    public string SourceUrl { get; set; } = "";
    public string Payload { get; set; } = "";
}
