using System.Collections;
using System.Reflection;
using System.Text.Json;
using WordWave.Api.Data;
using Xunit;

namespace WordWave.Api.Tests;

public class PatternCatalogTests
{
    [Fact]
    public void DailyCatalogContainsOneThousandCompleteUniqueSentencesAcrossTwentyTopics()
    {
        using var stream = typeof(PatternDataSeeder).Assembly.GetManifestResourceStream("WordWave.Api.Data.daily-patterns.json");
        Assert.NotNull(stream);
        using var document = JsonDocument.Parse(stream);
        var patterns = document.RootElement.EnumerateArray().ToArray();

        Assert.Equal(1000, patterns.Length);
        Assert.Equal(1000, patterns.Select(x => x.GetProperty("sentence").GetString()!.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal(20, patterns.GroupBy(x => x.GetProperty("type").GetString()).Count());
        Assert.All(patterns.GroupBy(x => x.GetProperty("type").GetString()), group => Assert.Equal(50, group.Count()));
        Assert.All(patterns, pattern =>
        {
            foreach (var field in new[] { "sentence", "type", "meaning", "explanation" })
            {
                Assert.False(string.IsNullOrWhiteSpace(pattern.GetProperty(field).GetString()));
            }

            Assert.Equal(pattern.GetProperty("sentence").GetString(), pattern.GetProperty("examples")[0].GetString());
            Assert.Equal(pattern.GetProperty("explanation").GetString(), pattern.GetProperty("examples")[1].GetString());
            Assert.Matches("[^\\u0000-\\u007F]", pattern.GetProperty("explanation").GetString()!);
        });
    }

    [Fact]
    public void AdditionalCatalogLoadsAndDoesNotOverlapTheFiveHundredLegacySentences()
    {
        var seeder = typeof(PatternDataSeeder);
        var legacyMethod = seeder.GetMethod("BuildDailyPatterns", BindingFlags.NonPublic | BindingFlags.Static)!;
        var additionalMethod = seeder.GetMethod("LoadAdditionalPatterns", BindingFlags.NonPublic | BindingFlags.Static)!;
        var legacy = ((IEnumerable)legacyMethod.Invoke(null, null)!).Cast<object>().Take(500).ToArray();
        var additional = ((IEnumerable)additionalMethod.Invoke(null, null)!).Cast<object>().ToArray();
        var sentences = legacy.Concat(additional).Select(x => (string)x.GetType().GetProperty("Sentence")!.GetValue(x)!);

        Assert.Equal(500, legacy.Length);
        Assert.Equal(1000, additional.Length);
        Assert.Equal(1500, sentences.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }
}
