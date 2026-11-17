using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace WordWave.Api.Tests;

public class VocabularyDataTests : IClassFixture<AuthFixture>
{
    private readonly AuthFixture _fixture;
    public VocabularyDataTests(AuthFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task ImportedDatasetSupportsPagingTopicLevelSearchAndRandomPractice()
    {
        await _fixture.ImportVocabularyPackageAsync();
        using var client = _fixture.CreateClient();
        await AuthFixture.LoginAsync(client, AuthFixture.AdminEmail);
        var page = await client.GetFromJsonAsync<JsonElement>("/api/vocabulary?limit=20");
        Assert.Equal(3001, page.GetProperty("total").GetInt32());
        Assert.Equal(20, page.GetProperty("data").GetArrayLength());
        Assert.All(page.GetProperty("data").EnumerateArray(), word =>
        {
            Assert.False(string.IsNullOrWhiteSpace(word.GetProperty("meaning").GetString()));
            Assert.False(string.IsNullOrWhiteSpace(word.GetProperty("phonetic").GetString()));
        });
        var topics = await client.GetFromJsonAsync<string[]>("/api/vocabulary/topics");
        Assert.Equal(32, topics!.Length);
        Assert.Contains("family", topics);
        Assert.Contains("technology", topics);
        var search = await client.GetFromJsonAsync<JsonElement>("/api/vocabulary?topic=technology&level=A1&search=computer");
        Assert.True(search.GetProperty("total").GetInt32() >= 1);
        Assert.All(search.GetProperty("data").EnumerateArray(), word =>
        {
            Assert.Equal("technology", word.GetProperty("topic").GetString());
            Assert.Equal("A1", word.GetProperty("level").GetString());
        });
        var random = await client.GetFromJsonAsync<JsonElement>("/api/vocabulary/random?topic=family&count=5");
        Assert.Equal(5, random.GetArrayLength());
        Assert.All(random.EnumerateArray(), word => Assert.Equal("family", word.GetProperty("topic").GetString()));
        var lastPage = await client.GetFromJsonAsync<JsonElement>("/api/vocabulary?page=151&limit=20");
        Assert.Equal(1, lastPage.GetProperty("data").GetArrayLength());
        var summaries = await client.GetFromJsonAsync<JsonElement>("/api/vocabulary/topics/summary");
        Assert.Equal(32, summaries.GetArrayLength());
        Assert.Equal(3000, summaries.EnumerateArray().Sum(topic => topic.GetProperty("count").GetInt32()));
        Assert.All(summaries.EnumerateArray(), topic => Assert.True(topic.GetProperty("levels").GetArrayLength() > 0));
        var beginnerSummaries = await client.GetFromJsonAsync<JsonElement>("/api/vocabulary/topics/summary?level=A1");
        Assert.Equal(722, beginnerSummaries.EnumerateArray().Sum(topic => topic.GetProperty("count").GetInt32()));
        Assert.All(beginnerSummaries.EnumerateArray(), topic => Assert.Equal(new[] { "A1" }, topic.GetProperty("levels").EnumerateArray().Select(level => level.GetString()).ToArray()));
        Assert.Empty((await client.GetFromJsonAsync<JsonElement>("/api/vocabulary/topics/summary?level=C1")).EnumerateArray());
    }
}
