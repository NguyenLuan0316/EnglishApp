using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace WordWave.Api.Tests;

public class AuthTests : IClassFixture<AuthFixture>
{
    private readonly AuthFixture _fixture;
    public AuthTests(AuthFixture fixture) => _fixture = fixture;
    private static string Email() => $"learner-{Guid.NewGuid():N}@wordwave.test";

    [Fact]
    public async Task AdminCanCreateEditAndResetPasswordWithValidationAndSessionRevocation()
    {
        using var admin = _fixture.CreateClient();
        using var learner = _fixture.CreateClient();
        await AuthFixture.LoginAsync(admin, AuthFixture.AdminEmail);
        var email = Email();
        var created = await AuthFixture.SendAsync(admin, "/api/admin/users", HttpMethod.Post, new { email, displayName = "Created by admin", password = AuthFixture.Password, role = "Learner", isActive = true });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var user = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = user.GetProperty("id").GetGuid();
        Assert.False(user.TryGetProperty("passwordHash", out _));
        Assert.Equal(HttpStatusCode.Conflict, (await AuthFixture.SendAsync(admin, "/api/admin/users", HttpMethod.Post, new { email = email.ToUpperInvariant(), displayName = "Duplicate", password = AuthFixture.Password })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await AuthFixture.SendAsync(admin, "/api/admin/users", HttpMethod.Post, new { email = Email(), displayName = "Invalid", password = "short", role = "Owner" })).StatusCode);
        await AuthFixture.LoginAsync(learner, email);
        Assert.Equal(HttpStatusCode.Conflict, (await AuthFixture.SendAsync(admin, $"/api/admin/users/{id}", HttpMethod.Put, new { email = AuthFixture.AdminEmail, displayName = "Duplicate email", role = "Learner", isActive = true })).StatusCode);
        var newEmail = Email();
        var updated = await AuthFixture.SendAsync(admin, $"/api/admin/users/{id}", HttpMethod.Put, new { email = newEmail, displayName = "Updated learner", role = "Learner", isActive = true });
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        Assert.Equal("Updated learner", (await updated.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("displayName").GetString());
        Assert.Equal(HttpStatusCode.Unauthorized, (await learner.GetAsync("/api/auth/me")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await AuthFixture.SendAsync(learner, "/api/auth/login", HttpMethod.Post, new { email, password = AuthFixture.Password })).StatusCode);
        await AuthFixture.LoginAsync(learner, newEmail);
        Assert.Equal(HttpStatusCode.BadRequest, (await AuthFixture.SendAsync(admin, $"/api/admin/users/{id}/password", HttpMethod.Put, new { newPassword = "short" })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await AuthFixture.SendAsync(admin, $"/api/admin/users/{id}/password", HttpMethod.Put, new { newPassword = "Reset-by-admin-1357!" })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await learner.GetAsync("/api/auth/me")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await AuthFixture.SendAsync(learner, "/api/auth/login", HttpMethod.Post, new { email = newEmail, password = AuthFixture.Password })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await AuthFixture.SendAsync(learner, "/api/auth/login", HttpMethod.Post, new { email = newEmail, password = "Reset-by-admin-1357!" })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await AuthFixture.SendAsync(admin, $"/api/admin/users/{Guid.NewGuid()}", HttpMethod.Put, new { email = Email(), displayName = "Missing", role = "Learner", isActive = true })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await AuthFixture.SendAsync(admin, $"/api/admin/users/{Guid.NewGuid()}/password", HttpMethod.Put, new { newPassword = AuthFixture.Password })).StatusCode);
    }

    [Fact]
    public async Task AdminDeletionRemovesAccountSessionsAndOwnedLearningData()
    {
        using var admin = _fixture.CreateClient();
        using var learner = _fixture.CreateClient();
        await AuthFixture.LoginAsync(admin, AuthFixture.AdminEmail);
        var email = Email();
        var id = await AuthFixture.RegisterAsync(learner, email);
        await AuthFixture.LoginAsync(learner, email);
        var tests = await learner.GetFromJsonAsync<JsonElement>("/api/ielts/tests");
        var testId = tests[0].GetProperty("id").GetInt32();
        Assert.Equal(HttpStatusCode.OK, (await AuthFixture.SendAsync(learner, $"/api/ielts/tests/{testId}/attempt", HttpMethod.Put, new { stateData = new { marker = "delete-account" } })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await AuthFixture.SendAsync(learner, "/api/review/submit", HttpMethod.Post, new { wordId = 999001, correct = true })).StatusCode);
        var self = (await admin.GetFromJsonAsync<JsonElement>("/api/auth/me")).GetProperty("id").GetGuid();
        Assert.Equal(HttpStatusCode.Conflict, (await AuthFixture.SendAsync(admin, $"/api/admin/users/{self}", HttpMethod.Delete, new { })).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await AuthFixture.SendAsync(admin, $"/api/admin/users/{self}/password", HttpMethod.Put, new { newPassword = AuthFixture.Password })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await AuthFixture.SendAsync(admin, $"/api/admin/users/{id}", HttpMethod.Delete, new { })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await learner.GetAsync("/api/auth/me")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await AuthFixture.SendAsync(learner, "/api/auth/login", HttpMethod.Post, new { email, password = AuthFixture.Password })).StatusCode);
        await _fixture.AssertAccountDeletedAsync(id);
        Assert.Equal(HttpStatusCode.NotFound, (await AuthFixture.SendAsync(admin, $"/api/admin/users/{id}", HttpMethod.Delete, new { })).StatusCode);
        Assert.NotEqual(id, await AuthFixture.RegisterAsync(learner, email));
    }

    [Fact]
    public async Task LearnerCannotCreateEditDeleteOrResetOtherAccounts()
    {
        using var learner = _fixture.CreateClient();
        var email = Email();
        var id = await AuthFixture.RegisterAsync(learner, email);
        await AuthFixture.LoginAsync(learner, email);
        Assert.Equal(HttpStatusCode.Forbidden, (await AuthFixture.SendAsync(learner, "/api/admin/users", HttpMethod.Post, new { email = Email(), displayName = "Denied", password = AuthFixture.Password, role = "Admin" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await AuthFixture.SendAsync(learner, $"/api/admin/users/{id}", HttpMethod.Put, new { email, displayName = "Denied", role = "Admin", isActive = true })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await AuthFixture.SendAsync(learner, $"/api/admin/users/{id}", HttpMethod.Delete, new { })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await AuthFixture.SendAsync(learner, $"/api/admin/users/{id}/password", HttpMethod.Put, new { newPassword = AuthFixture.Password })).StatusCode);
    }

    [Fact]
    public async Task AnonymousAccessRequiresLoginAndMutationsRequireCsrf()
    {
        using var client = _fixture.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/vocabulary")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/admin/users")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/auth/login", new { email = AuthFixture.AdminEmail, password = AuthFixture.Password })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await AuthFixture.SendAsync(client, "/api/auth/register", HttpMethod.Post, new { email = "bad", displayName = " ", password = "short" })).StatusCode);
    }

    [Fact]
    public async Task RegistrationCannotGrantAdminAndEmailsAreUniqueIgnoringCase()
    {
        using var client = _fixture.CreateClient();
        var email = Email();
        await AuthFixture.RegisterAsync(client, email);
        Assert.Equal(HttpStatusCode.Conflict, (await AuthFixture.SendAsync(client, "/api/auth/register", HttpMethod.Post, new { email = email.ToUpperInvariant(), displayName = "Other", password = AuthFixture.Password })).StatusCode);
        await AuthFixture.LoginAsync(client, email);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/vocabulary")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/review/submit", new { wordId = 999001, correct = true })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/admin/users")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/admin/toeic/import-logs")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await AuthFixture.SendAsync(client, "/api/admin/toeic/crawl", HttpMethod.Post, new { keyword = "test" })).StatusCode);
    }

    [Fact]
    public async Task LogoutRevokesCopiedCookieAndPasswordChangeRevokesAllSessions()
    {
        using var client = _fixture.CreateClient();
        using var second = _fixture.CreateClient();
        var email = Email();
        await AuthFixture.RegisterAsync(client, email);
        var login = await AuthFixture.SendAsync(client, "/api/auth/login", HttpMethod.Post, new { email, password = AuthFixture.Password });
        var cookie = login.Headers.GetValues("Set-Cookie").Single(x => x.StartsWith("wordwave.session=")).Split(';')[0];
        Assert.Contains("httponly", login.Headers.GetValues("Set-Cookie").Single(x => x.StartsWith("wordwave.session=")).ToLowerInvariant());
        await AuthFixture.LoginAsync(second, email);
        Assert.Equal(HttpStatusCode.NoContent, (await AuthFixture.SendAsync(client, "/api/auth/logout", HttpMethod.Post, new { })).StatusCode);
        using var replay = _fixture.CreateClient();
        replay.DefaultRequestHeaders.Add("Cookie", cookie);
        Assert.Equal(HttpStatusCode.Unauthorized, (await replay.GetAsync("/api/auth/me")).StatusCode);
        await AuthFixture.LoginAsync(client, email);
        Assert.Equal(HttpStatusCode.BadRequest, (await AuthFixture.SendAsync(client, "/api/auth/password", HttpMethod.Put, new { currentPassword = "wrong", newPassword = "New-password-8754!" })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await AuthFixture.SendAsync(client, "/api/auth/password", HttpMethod.Put, new { currentPassword = AuthFixture.Password, newPassword = "New-password-8754!" })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await second.GetAsync("/api/auth/me")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await AuthFixture.SendAsync(client, "/api/auth/login", HttpMethod.Post, new { email, password = AuthFixture.Password })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await AuthFixture.SendAsync(client, "/api/auth/login", HttpMethod.Post, new { email, password = "New-password-8754!" })).StatusCode);
    }

    [Fact]
    public async Task AdminCanChangeAccessAndExistingSessionsAreImmediatelyRevoked()
    {
        using var admin = _fixture.CreateClient();
        using var learner = _fixture.CreateClient();
        var email = Email();
        var id = await AuthFixture.RegisterAsync(learner, email);
        await AuthFixture.LoginAsync(learner, email);
        await AuthFixture.LoginAsync(admin, AuthFixture.AdminEmail);
        var users = await admin.GetFromJsonAsync<JsonElement>("/api/admin/users");
        var self = users.EnumerateArray().Single(x => x.GetProperty("email").GetString() == AuthFixture.AdminEmail).GetProperty("id").GetGuid();
        Assert.Equal(HttpStatusCode.Conflict, (await AuthFixture.SendAsync(admin, $"/api/admin/users/{self}/access", HttpMethod.Put, new { role = "Learner", isActive = true })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await AuthFixture.SendAsync(admin, $"/api/admin/users/{id}/access", HttpMethod.Put, new { role = "Owner", isActive = true })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await AuthFixture.SendAsync(admin, $"/api/admin/users/{id}/access", HttpMethod.Put, new { role = "Admin", isActive = true })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await learner.GetAsync("/api/auth/me")).StatusCode);
        await AuthFixture.LoginAsync(learner, email);
        Assert.Equal(HttpStatusCode.OK, (await learner.GetAsync("/api/admin/users")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await AuthFixture.SendAsync(admin, $"/api/admin/users/{id}/access", HttpMethod.Put, new { role = "Learner", isActive = false })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await learner.GetAsync("/api/auth/me")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await AuthFixture.SendAsync(learner, "/api/auth/login", HttpMethod.Post, new { email, password = AuthFixture.Password })).StatusCode);
    }

    [Fact]
    public async Task IeltsAttemptsUseServerIdentityEvenWhenLearnerIdIsSpoofed()
    {
        using var first = _fixture.CreateClient();
        using var second = _fixture.CreateClient();
        var firstEmail = Email();
        var secondEmail = Email();
        var firstId = await AuthFixture.RegisterAsync(first, firstEmail);
        var secondId = await AuthFixture.RegisterAsync(second, secondEmail);
        await AuthFixture.LoginAsync(first, firstEmail);
        await AuthFixture.LoginAsync(second, secondEmail);
        var tests = await first.GetFromJsonAsync<JsonElement>("/api/ielts/tests");
        var id = tests[0].GetProperty("id").GetInt32();
        var saved = await AuthFixture.SendAsync(first, $"/api/ielts/tests/{id}/attempt", HttpMethod.Put, new { learnerId = secondId.ToString(), stateData = new { marker = "owned-by-first" } });
        Assert.Equal(HttpStatusCode.OK, saved.StatusCode);
        var own = await first.GetAsync($"/api/ielts/tests/{id}/attempt?learnerId={secondId}");
        Assert.Equal(HttpStatusCode.OK, own.StatusCode);
        Assert.Contains("owned-by-first", await own.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.NotFound, (await second.GetAsync($"/api/ielts/tests/{id}/attempt?learnerId={firstId}")).StatusCode);
    }

    [Fact]
    public async Task ReviewProgressBelongsToEachUserAndSurvivesNewSessions()
    {
        using var first = _fixture.CreateClient();
        using var second = _fixture.CreateClient();
        var email = Email();
        var otherEmail = Email();
        await AuthFixture.RegisterAsync(first, email);
        await AuthFixture.RegisterAsync(second, otherEmail);
        await AuthFixture.LoginAsync(first, email);
        await AuthFixture.LoginAsync(second, otherEmail);
        var submit = await AuthFixture.SendAsync(first, "/api/review/submit", HttpMethod.Post, new { wordId = 999001, correct = true });
        Assert.True(submit.IsSuccessStatusCode, await submit.Content.ReadAsStringAsync());
        Assert.Equal(1, (await first.GetFromJsonAsync<JsonElement>("/api/review/progress")).GetProperty("learned").GetInt32());
        Assert.Equal(0, (await second.GetFromJsonAsync<JsonElement>("/api/review/progress")).GetProperty("learned").GetInt32());
        Assert.Equal(HttpStatusCode.NotFound, (await AuthFixture.SendAsync(first, "/api/review/submit", HttpMethod.Post, new { wordId = int.MaxValue, correct = true })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await AuthFixture.SendAsync(first, "/api/review/submit", HttpMethod.Post, new { wordId = 0, correct = true })).StatusCode);
        await AuthFixture.SendAsync(first, "/api/auth/logout", HttpMethod.Post, new { });
        await AuthFixture.LoginAsync(first, email);
        Assert.Equal(1, (await first.GetFromJsonAsync<JsonElement>("/api/review/progress")).GetProperty("learned").GetInt32());
    }

    [Fact]
    public async Task FailedLoginsTemporarilyLockAccount()
    {
        using var client = _fixture.CreateClient();
        var email = Email();
        await AuthFixture.RegisterAsync(client, email);
        var csrf = await client.GetFromJsonAsync<JsonElement>("/api/auth/csrf");
        var attempts = Enumerable.Range(0, 5).Select(async _ =>
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login") { Content = JsonContent.Create(new { email, password = "wrong" }) };
            request.Headers.Add("X-CSRF-TOKEN", csrf.GetProperty("token").GetString());
            using var response = await client.SendAsync(request);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        });
        await Task.WhenAll(attempts);
        Assert.Equal(HttpStatusCode.Unauthorized, (await AuthFixture.SendAsync(client, "/api/auth/login", HttpMethod.Post, new { email, password = AuthFixture.Password })).StatusCode);
    }
}
