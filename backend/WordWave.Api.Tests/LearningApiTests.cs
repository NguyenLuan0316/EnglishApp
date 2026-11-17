using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using WordWave.Infrastructure.Data;
using Xunit;

namespace WordWave.Api.Tests;

public sealed class LearningApiFixture : IAsyncLifetime
{
    private readonly LearningStoreFixture _store = new();
    private WebApplicationFactory<Program> _factory = null!;
    public async Task InitializeAsync()
    {
        await _store.InitializeAsync();
        await using var auth = _store.CreateContext();
        var connection = auth.Database.GetConnectionString()!;
        await using (var content = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(connection).Options))
        {
            // Both schema scripts target this fixture's new disposable database, never an existing database.
            await content.Database.ExecuteSqlRawAsync(content.Database.GenerateCreateScript());
            content.Vocabulary.AddRange(Enumerable.Range(1, 20).Select(id => new WordWave.Domain.Models.VocabWord { Id = id, Word = $"learning-{id}", Meaning = "nghĩa", Level = "A1", Topic = "daily" }));
            await content.SaveChangesAsync();
        }
        var root = Environment.GetEnvironmentVariable("WORDWAVE_REPO_ROOT") ?? Path.GetFullPath("../../../../../", AppContext.BaseDirectory);
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(web =>
        {
            web.UseEnvironment("Development");
            web.UseContentRoot(Path.Combine(root, "backend/WordWave.Api"));
            web.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:DefaultConnection"] = connection, ["Auth:AdminEmail"] = "", ["Auth:AdminPassword"] = "" }));
            web.ConfigureServices(services =>
            {
                services.RemoveAll<IConfigureOptions<Microsoft.AspNetCore.RateLimiting.RateLimiterOptions>>();
                services.Configure<Microsoft.AspNetCore.RateLimiting.RateLimiterOptions>(options => options.AddPolicy("auth", _ => System.Threading.RateLimiting.RateLimitPartition.GetNoLimiter("learning-tests")));
            });
        });
    }
    public HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, BaseAddress = new Uri("http://localhost"), HandleCookies = true });
    public async Task DisposeAsync()
    {
        if (_factory is not null) await _factory.DisposeAsync();
        await _store.DisposeAsync();
    }
}

public class LearningApiTests : IClassFixture<LearningApiFixture>
{
    private readonly LearningApiFixture _fixture;
    public LearningApiTests(LearningApiFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task DailyLearningAndDashboardShareAccountDataAndEnforceAuthenticationAndValidation()
    {
        using var anonymous = _fixture.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/learning/daily")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/learning/progress")).StatusCode);
        using var first = _fixture.CreateClient();
        using var second = _fixture.CreateClient();
        var firstEmail = $"{Guid.NewGuid():N}@learning.test";
        var secondEmail = $"{Guid.NewGuid():N}@learning.test";
        await AuthFixture.RegisterAsync(first, firstEmail);
        var secondId = await AuthFixture.RegisterAsync(second, secondEmail);
        await AuthFixture.LoginAsync(first, firstEmail);
        await AuthFixture.LoginAsync(second, secondEmail);
        var plan = await first.GetFromJsonAsync<JsonElement>("/api/learning/daily");
        Assert.Equal(15, plan.GetProperty("total").GetInt32());
        foreach (var type in new[] { "vocabulary", "grammar", "patterns" })
        {
            var item = plan.GetProperty("lessons").EnumerateArray().First(x => x.GetProperty("contentType").GetString() == type);
            var body = new { contentType = type, contentId = item.GetProperty("contentId").GetInt32(), correct = true, daily = true, userId = secondId };
            var saved = await AuthFixture.SendAsync(first, "/api/learning/submit", HttpMethod.Post, body);
            Assert.Equal(HttpStatusCode.OK, saved.StatusCode);
            var again = await AuthFixture.SendAsync(first, "/api/learning/submit", HttpMethod.Post, body);
            Assert.False((await again.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("recorded").GetBoolean());
        }
        var summary = await first.GetFromJsonAsync<JsonElement>("/api/learning/progress");
        Assert.Equal(3, summary.GetProperty("totalLearned").GetInt32());
        Assert.Equal(3, summary.GetProperty("dailyCompleted").GetInt32());
        Assert.Equal(30, summary.GetProperty("totalXp").GetInt32());
        Assert.Equal(1, summary.GetProperty("streak").GetInt32());
        Assert.Equal(1, summary.GetProperty("byContent").GetProperty("grammar").GetProperty("learned").GetInt32());
        Assert.Equal(1, (await first.GetFromJsonAsync<JsonElement>("/api/review/progress")).GetProperty("learned").GetInt32());
        var untouched = await second.GetFromJsonAsync<JsonElement>("/api/learning/progress");
        Assert.Equal(0, untouched.GetProperty("totalLearned").GetInt32());
        Assert.Equal(0, untouched.GetProperty("totalXp").GetInt32());
        Assert.Equal(0, untouched.GetProperty("dailyCompleted").GetInt32());
        var reloaded = await first.GetFromJsonAsync<JsonElement>("/api/learning/daily");
        Assert.Equal(3, reloaded.GetProperty("completed").GetInt32());
        Assert.Equal(HttpStatusCode.Forbidden, (await first.PostAsJsonAsync("/api/learning/submit", new { contentType = "grammar", contentId = 1, correct = true })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await AuthFixture.SendAsync(first, "/api/learning/submit", HttpMethod.Post, new { contentType = "invalid", contentId = 0, correct = true })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await AuthFixture.SendAsync(first, "/api/learning/submit", HttpMethod.Post, new { contentType = "grammar", contentId = int.MaxValue, correct = true })).StatusCode);
        var secondPlan = await second.GetFromJsonAsync<JsonElement>("/api/learning/daily");
        var plannedWords = secondPlan.GetProperty("lessons").EnumerateArray().Where(x => x.GetProperty("contentType").GetString() == "vocabulary").Select(x => x.GetProperty("contentId").GetInt32()).ToHashSet();
        var unplanned = Enumerable.Range(1, 20).First(id => !plannedWords.Contains(id));
        Assert.Equal(HttpStatusCode.Conflict, (await AuthFixture.SendAsync(second, "/api/learning/submit", HttpMethod.Post, new { contentType = "vocabulary", contentId = unplanned, correct = true, daily = true })).StatusCode);
    }
}
