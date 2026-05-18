using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
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
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Supabase")));

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
         .AllowAnyHeader()));

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
app.MapControllers();

// Health check
app.MapGet("/api/health", () => new { status = "ok", time = DateTime.UtcNow });

try
{
    await app.SeedGrammarDataAsync();
    await app.SeedPatternDataAsync();
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Failed to seed startup data.");
}

Console.WriteLine("\n🌊 WordWave API đang chạy tại http://localhost:5000");
Console.WriteLine("   Swagger/test: http://localhost:5000/api/health\n");

var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
app.Run($"http://0.0.0.0:{port}");
