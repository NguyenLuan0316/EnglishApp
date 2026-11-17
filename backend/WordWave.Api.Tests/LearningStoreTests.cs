using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using WordWave.Domain.Models;
using WordWave.Infrastructure.Data;
using WordWave.Infrastructure.Repositories;
using Xunit;

namespace WordWave.Api.Tests;

// Uses only EnsureCreated on a newly generated localhost test database; never migrates an existing database.
public sealed class LearningStoreFixture : IAsyncLifetime
{
    private readonly string _database = $"wordwave_learning_test_{Guid.NewGuid():N}";
    private string _adminConnection = "";
    private string _connection = "";
    public AuthDbContext CreateContext() => new(new DbContextOptionsBuilder<AuthDbContext>().UseNpgsql(_connection).Options);

    public async Task InitializeAsync()
    {
        var root = Environment.GetEnvironmentVariable("WORDWAVE_REPO_ROOT") ?? Path.GetFullPath("../../../../../", AppContext.BaseDirectory);
        var config = new ConfigurationBuilder().SetBasePath(Path.Combine(root, "backend/WordWave.Api")).AddJsonFile("appsettings.json").Build();
        var builder = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("WORDWAVE_TEST_CONNECTION") ?? config.GetConnectionString("DefaultConnection"));
        if (builder.Host is not ("localhost" or "127.0.0.1")) throw new InvalidOperationException("Learning tests require a disposable localhost database.");
        builder.Database = "postgres";
        _adminConnection = builder.ConnectionString;
        await using (var connection = new NpgsqlConnection(_adminConnection))
        {
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand($"CREATE DATABASE \"{_database}\"", connection);
            await command.ExecuteNonQueryAsync();
        }
        builder.Database = _database;
        _connection = builder.ConnectionString;
        await using var db = CreateContext();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task<Guid> AddUserAsync()
    {
        await using var db = CreateContext();
        var user = new AppUser { Email = $"{Guid.NewGuid():N}@learning.test", NormalizedEmail = Guid.NewGuid().ToString(), DisplayName = "Learner", PasswordHash = "test-only" };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user.Id;
    }

    public async Task DisposeAsync()
    {
        if (string.IsNullOrEmpty(_adminConnection)) return;
        await using var connection = new NpgsqlConnection(_adminConnection);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{_database}\" WITH (FORCE)", connection);
        await command.ExecuteNonQueryAsync();
    }
}

public class LearningStoreTests : IClassFixture<LearningStoreFixture>
{
    private readonly LearningStoreFixture _fixture;
    public LearningStoreTests(LearningStoreFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task ConcurrentDailyCompletionPersistsExactlyOneAttemptAndSurvivesNewContext()
    {
        var user = await _fixture.AddUserAsync();
        var now = DateTime.UtcNow;
        var date = StudySchedule.Date(now);
        await using (var db = _fixture.CreateContext())
        {
            await new UserLearningRepository(db).GetOrCreatePlanAsync(user, date, [new() { ContentType = "grammar", ContentId = 8 }]);
        }
        var results = await Task.WhenAll(Enumerable.Range(0, 6).Select(async _ =>
        {
            await using var db = _fixture.CreateContext();
            return await new UserLearningRepository(db).RecordAsync(user, "grammar", 8, true, true, now);
        }));
        Assert.Single(results, x => x);
        await using var verify = _fixture.CreateContext();
        var repository = new UserLearningRepository(verify);
        var progress = Assert.Single(await repository.GetProgressAsync(user));
        Assert.Equal(1, progress.CorrectCount);
        var activity = Assert.Single(await repository.GetActivityAsync(user));
        Assert.Equal(1, activity.Attempts);
        Assert.Equal(10, activity.Xp);
        Assert.NotNull((await verify.DailyLessons.SingleAsync(x => x.UserId == user)).CompletedAt);
    }

    [Fact]
    public async Task AccountsAndContentTypesStaySeparateAndDeletionCascades()
    {
        var first = await _fixture.AddUserAsync();
        var second = await _fixture.AddUserAsync();
        await using var db = _fixture.CreateContext();
        var repository = new UserLearningRepository(db);
        foreach (var type in new[] { "vocabulary", "grammar", "patterns" }) await repository.RecordAsync(first, type, 11, true, false, DateTime.UtcNow);
        Assert.Equal(3, (await repository.GetProgressAsync(first)).Count);
        Assert.Empty(await repository.GetProgressAsync(second));
        Assert.Empty(await repository.GetActivityAsync(second));
        Assert.Single(await db.WordProgress.Where(x => x.UserId == first).ToListAsync());
        Assert.Equal(2, await db.ContentProgress.CountAsync(x => x.UserId == first));
        await repository.GetOrCreatePlanAsync(first, StudySchedule.Date(DateTime.UtcNow), [new() { ContentType = "patterns", ContentId = 12 }]);
        await db.Users.Where(x => x.Id == first).ExecuteDeleteAsync();
        Assert.Empty(await repository.GetProgressAsync(first));
        Assert.Empty(await repository.GetActivityAsync(first));
        Assert.False(await db.DailyLessons.AnyAsync(x => x.UserId == first));
    }

    [Fact]
    public async Task InvalidDailySubmissionRollsBackAndConcurrentPlanCreationIsStable()
    {
        var user = await _fixture.AddUserAsync();
        var date = StudySchedule.Date(DateTime.UtcNow);
        var plans = await Task.WhenAll(Enumerable.Range(1, 4).Select(async index =>
        {
            await using var db = _fixture.CreateContext();
            return await new UserLearningRepository(db).GetOrCreatePlanAsync(user, date, [new() { ContentType = "patterns", ContentId = index }]);
        }));
        Assert.Single(plans.Select(plan => plan.Single().ContentId).Distinct());
        await using var verify = _fixture.CreateContext();
        var repository = new UserLearningRepository(verify);
        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.RecordAsync(user, "grammar", 9999, true, true, DateTime.UtcNow));
        Assert.Empty(await repository.GetProgressAsync(user));
        Assert.Empty(await repository.GetActivityAsync(user));
    }

    [Fact]
    public async Task LegacyVocabularyReviewAlsoUpdatesDailyActivityAndKeepsExistingCounts()
    {
        var user = await _fixture.AddUserAsync();
        await using var db = _fixture.CreateContext();
        db.WordProgress.Add(new() { UserId = user, WordId = 4, CorrectCount = 4, NextReview = DateTime.UtcNow });
        await db.SaveChangesAsync();
        var learning = new UserLearningRepository(db);
        var legacy = new UserProgressRepository(db, learning, TimeProvider.System);
        var result = await legacy.RecordAsync(user, 4, false);
        Assert.Equal(4, result.CorrectCount);
        Assert.Equal(1, result.WrongCount);
        Assert.Equal(result.LastReviewed, result.NextReview);
        var activity = Assert.Single(await learning.GetActivityAsync(user));
        Assert.Equal(1, activity.Attempts);
        Assert.Equal(0, activity.Xp);
    }
}
