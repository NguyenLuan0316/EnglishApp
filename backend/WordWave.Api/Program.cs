using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Threading.RateLimiting;
using WordWave.Api.Security;
using WordWave.Infrastructure.Security;
using WordWave.Api.Data;
using WordWave.Application.Interfaces;
using WordWave.Application.Interfaces.Repositories;
using WordWave.Application.Services;
using WordWave.Infrastructure.Data;
using WordWave.Infrastructure.Repositories;
using WordWave.Infrastructure.Toeic;

var builder = WebApplication.CreateBuilder(args);
AppContext.SetSwitch("System.Net.DisableIPv6", true);
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") ?? builder.Configuration.GetConnectionString("Supabase")));
builder.Services.AddDbContext<AuthDbContext>(opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") ?? builder.Configuration.GetConnectionString("Supabase"), npgsql => npgsql.MigrationsHistoryTable("__AuthMigrationsHistory")));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserProgressRepository, UserProgressRepository>();
builder.Services.AddScoped<IUserLearningRepository, UserLearningRepository>();
builder.Services.AddScoped<ILearningService, LearningService>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IPasswordService, PasswordService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<SessionCookieEvents>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.Cookie.Name = "wordwave.session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = builder.Environment.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = false;
    options.EventsType = typeof(SessionCookieEvents);
});
builder.Services.AddAuthorization(options => options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "wordwave.csrf";
    options.Cookie.SameSite = builder.Environment.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});

builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(opt =>
{
    opt.InvalidModelStateResponseFactory = context =>
    {
        var details = new ValidationProblemDetails(context.ModelState)
        {
            Type = "https://httpstatuses.com/400",
            Title = "Validation Failed",
            Status = StatusCodes.Status400BadRequest,
            Detail = "One or more validation errors occurred.",
            Instance = context.HttpContext.Request.Path
        };
        details.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        return new BadRequestObjectResult(details);
    };
});
builder.Services.AddProblemDetails();
builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p =>
        p.WithOrigins("http://localhost:5173",   
                      "http://localhost:5500",
                      "http://127.0.0.1:5500",
                      "https://englishapp-er2b.onrender.com",
                      "https://englishapp-luan.netlify.app")                   
         .AllowAnyMethod()
         .AllowAnyHeader()
         .AllowCredentials()));

builder.Services.AddEndpointsApiExplorer();

// Dependency Injection
builder.Services.AddScoped<IGrammarService, GrammarService>();
builder.Services.AddScoped<IGrammarRepository, GrammarRepository>();
// New services and repositories
builder.Services.AddScoped<IVocabularyService, VocabularyService>();
builder.Services.AddScoped<IVocabularyRepository, VocabularyRepository>();
builder.Services.AddScoped<IPatternService, PatternService>();
builder.Services.AddScoped<IPatternRepository, PatternRepository>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IToeicService, ToeicService>();
builder.Services.AddScoped<IToeicImportService, ToeicImportService>();
builder.Services.AddScoped<IToeicImportPackageWriter, ToeicImportPackageWriter>();
builder.Services.AddScoped<IToeicRepository, ToeicRepository>();
builder.Services.AddScoped<IIeltsService, IeltsService>();
builder.Services.AddScoped<IIeltsRepository, IeltsRepository>();
builder.Services.AddScoped<IToeicImporter, JsonToeicImporter>();
builder.Services.AddScoped<IToeicImporter, CsvToeicImporter>();
builder.Services.AddScoped<IToeicImporter, AiGeneratedToeicImporter>();
builder.Services.AddHttpClient<IToeicDataSourceCrawler, WebsiteToeicCrawler>();

var app = builder.Build();

app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async context =>
    {
        var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://httpstatuses.com/500",
            title = "Internal Server Error",
            status = 500,
            detail = app.Environment.IsDevelopment() ? ex?.Message : "An unexpected error occurred.",
            traceId = context.TraceIdentifier
        });
    });
});

app.UseCors();
app.UseAuthentication();
app.UseRateLimiter();
app.UseStatusCodePages(async statusContext =>
{
    var response = statusContext.HttpContext.Response;
    if (response.HasStarted || response.StatusCode < 400)
    {
        return;
    }

    if (!string.IsNullOrWhiteSpace(response.ContentType))
    {
        return;
    }

    response.ContentType = "application/problem+json";
    var title = response.StatusCode switch
    {
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Resource Not Found",
        _ => "Request Failed"
    };

    await response.WriteAsJsonAsync(new ProblemDetails
    {
        Type = $"https://httpstatuses.com/{response.StatusCode}",
        Title = title,
        Status = response.StatusCode,
        Detail = $"HTTP {response.StatusCode} at {statusContext.HttpContext.Request.Path}",
        Instance = statusContext.HttpContext.Request.Path
    });
});
app.UseAuthorization();
app.Use(async (context, next) =>
{
    if (HttpMethods.IsPost(context.Request.Method) || HttpMethods.IsPut(context.Request.Method) || HttpMethods.IsPatch(context.Request.Method) || HttpMethods.IsDelete(context.Request.Method))
    {
        try
        {
            await context.RequestServices.GetRequiredService<IAntiforgery>().ValidateRequestAsync(context);
        }
        catch (AntiforgeryValidationException)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new ProblemDetails { Status = 403, Title = "Invalid request verification token", Detail = "Phiên xác thực yêu cầu đã thay đổi. Vui lòng tải lại trang và thử lại." });
            return;
        }
    }
    context.Response.Headers.CacheControl = "no-store";
    await next();
});
app.MapControllers();

// Health check
app.MapGet("/api/health", () => new { status = "ok", time = DateTime.UtcNow }).AllowAnonymous();

var bootstrapEmail = builder.Configuration["Auth:AdminEmail"];
var bootstrapPassword = builder.Configuration["Auth:AdminPassword"];
if (!string.IsNullOrWhiteSpace(bootstrapEmail) && !string.IsNullOrWhiteSpace(bootstrapPassword))
{
    if (bootstrapPassword.Length < 10 || bootstrapPassword.Length > 128 || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(bootstrapEmail))
    {
        throw new InvalidOperationException("Configure a valid Auth:AdminEmail and an Auth:AdminPassword of 10–128 characters.");
    }
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<IAuthService>().BootstrapAdminAsync(bootstrapEmail, "Quản trị viên", bootstrapPassword);
}

try
{
    await app.SeedGrammarDataAsync();
    await app.SeedPatternDataAsync();
    await app.SeedIeltsDataAsync();
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Failed to seed startup data.");
}

Console.WriteLine("\n🌊 WordWave API đang chạy tại http://localhost:5000");
Console.WriteLine("   Swagger/test: http://localhost:5000/api/health\n");

var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
app.Run($"http://0.0.0.0:{port}");

public partial class Program { }
