using Microsoft.EntityFrameworkCore;
using WordWave.Application.Interfaces;
using WordWave.Application.Interfaces.Repositories;
using WordWave.Application.Services;
using WordWave.Infrastructure.Data;
using WordWave.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);
AppContext.SetSwitch("System.Net.DisableIPv6", true);
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Supabase")));

builder.Services.AddControllers();
builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p =>
        p.WithOrigins("http://localhost:5173",   
                      "http://localhost:5500",
                      "http://127.0.0.1:5500",
                      "https://englishapp-er2b.onrender.com",
                      "https://englishapp-luan.netlify.app")                   
         .AllowAnyMethod()));

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

var app = builder.Build();

app.UseCors();
app.MapControllers();

// Health check
app.MapGet("/api/health", () => new { status = "ok", time = DateTime.UtcNow });

Console.WriteLine("\n🌊 WordWave API đang chạy tại http://localhost:5000");
Console.WriteLine("   Swagger/test: http://localhost:5000/api/health\n");

var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
app.Run($"http://0.0.0.0:{port}");