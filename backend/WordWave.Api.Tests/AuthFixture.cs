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
using Npgsql;
using WordWave.Infrastructure.Data;
using Xunit;

namespace WordWave.Api.Tests;

public sealed class AuthFixture : IAsyncLifetime
{
    private WebApplicationFactory<Program> _factory = null!;
    private string _adminConnection = "";
    private readonly string _database = $"wordwave_auth_test_{Guid.NewGuid():N}";
    public const string AdminEmail = "admin@wordwave.test";
    public const string Password = "Test-only-password-8754!";
    private static string RepositoryRoot => Environment.GetEnvironmentVariable("WORDWAVE_REPO_ROOT") ?? Path.GetFullPath("../../../../../", AppContext.BaseDirectory);

    public async Task InitializeAsync()
    {
        var config = new ConfigurationBuilder().SetBasePath(Path.Combine(RepositoryRoot, "backend/WordWave.Api")).AddJsonFile("appsettings.json").Build();
        var builder = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("WORDWAVE_TEST_CONNECTION") ?? config.GetConnectionString("DefaultConnection"));
        if (builder.Host is not ("127.0.0.1" or "localhost"))
        {
            throw new InvalidOperationException("Integration tests only provision disposable databases on localhost.");
        }
        builder.Database = "postgres";
        _adminConnection = builder.ConnectionString;
        await using (var connection = new NpgsqlConnection(_adminConnection))
        {
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand($"CREATE DATABASE \"{_database}\"", connection);
            await command.ExecuteNonQueryAsync();
        }
        builder.Database = _database;
        var connectionString = builder.ConnectionString;
        await using (var core = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(connectionString).Options))
        {
            await core.Database.EnsureCreatedAsync();
            core.Vocabulary.Add(new() { Id = 999001, Word = "testword", Meaning = "test", Level = "A1" });
            await core.SaveChangesAsync();
        }
        await using (var auth = new AuthDbContext(new DbContextOptionsBuilder<AuthDbContext>().UseNpgsql(connectionString, x => x.MigrationsHistoryTable("__AuthMigrationsHistory")).Options))
        {
            await auth.Database.MigrateAsync();
        }
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(web =>
        {
            web.UseEnvironment("Development");
            web.UseContentRoot(Path.Combine(RepositoryRoot, "backend/WordWave.Api"));
            web.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString,
                ["Auth:AdminEmail"] = AdminEmail,
                ["Auth:AdminPassword"] = Password
            }));
            web.ConfigureServices(services =>
            {
                // Test many independent cases without waiting for the interactive login throttle.
                services.RemoveAll<IConfigureOptions<Microsoft.AspNetCore.RateLimiting.RateLimiterOptions>>();
                services.Configure<Microsoft.AspNetCore.RateLimiting.RateLimiterOptions>(options => options.AddPolicy("auth", _ => System.Threading.RateLimiting.RateLimitPartition.GetNoLimiter("integration")));
            });
        });
        using var client = CreateClient();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/health")).StatusCode);
    }

    public HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true, BaseAddress = new Uri("http://localhost") });

    public static async Task<HttpResponseMessage> SendAsync(HttpClient client, string path, HttpMethod method, object body)
    {
        var csrf = await client.GetFromJsonAsync<JsonElement>("/api/auth/csrf");
        var request = new HttpRequestMessage(method, path) { Content = JsonContent.Create(body) };
        request.Headers.Add("X-CSRF-TOKEN", csrf.GetProperty("token").GetString());
        return await client.SendAsync(request);
    }

    public static async Task<Guid> RegisterAsync(HttpClient client, string email)
    {
        var response = await SendAsync(client, "/api/auth/register", HttpMethod.Post, new { email, displayName = "Test learner", password = Password, role = "Admin" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Learner", user.GetProperty("role").GetString());
        return user.GetProperty("id").GetGuid();
    }

    public static async Task LoginAsync(HttpClient client, string email)
    {
        var response = await SendAsync(client, "/api/auth/login", HttpMethod.Post, new { email, password = Password });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    public async Task DisposeAsync()
    {
        if (_factory is not null) await _factory.DisposeAsync();
        NpgsqlConnection.ClearAllPools();
        if (string.IsNullOrEmpty(_adminConnection)) return;
        await using var connection = new NpgsqlConnection(_adminConnection);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{_database}\" WITH (FORCE)", connection);
        await command.ExecuteNonQueryAsync();
    }

    public async Task AssertAccountDeletedAsync(Guid userId)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var auth = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var content = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.False(await auth.Users.AnyAsync(x => x.Id == userId));
        Assert.False(await auth.Sessions.AnyAsync(x => x.UserId == userId));
        Assert.False(await auth.WordProgress.AnyAsync(x => x.UserId == userId));
        Assert.False(await content.IeltsAttempts.AnyAsync(x => x.LearnerId == userId.ToString()));
    }

    public async Task ImportVocabularyPackageAsync()
    {
        var path = Path.Combine(RepositoryRoot, "frontend/data/vocabulary/words-3000.json");
        using var package = JsonDocument.Parse(await File.ReadAllTextAsync(path));
        await using var scope = _factory.Services.CreateAsyncScope();
        var content = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        foreach (var entry in package.RootElement.GetProperty("entries").EnumerateArray())
        {
            content.Vocabulary.Add(new()
            {
                Word = entry.GetProperty("word").GetString()!,
                Phonetic = entry.GetProperty("phonetic").GetString()!,
                Meaning = entry.GetProperty("meaning").GetString()!,
                Example = entry.GetProperty("example").GetString()!,
                ExampleMeaning = entry.GetProperty("exampleMeaning").GetString()!,
                Level = entry.GetProperty("level").GetString()!,
                Topic = entry.GetProperty("topic").GetString()!
            });
        }
        await content.SaveChangesAsync();
    }
}
