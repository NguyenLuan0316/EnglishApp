namespace WordWave.Infrastructure.Toeic;

public sealed class AiGeneratedToeicImporter : JsonToeicImporter
{
    public override string SourceType => "ai-generated";
}
