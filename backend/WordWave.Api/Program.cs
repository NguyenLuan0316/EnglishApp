// wordwave/backend/WordWave.Api/Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p =>
        p.WithOrigins("http://localhost:5500",   // Live Server (VSCode)
                      "http://127.0.0.1:5500",
                      "http://localhost:3000",
                      "null")                    // file:// khi mở trực tiếp
         .AllowAnyHeader()
         .AllowAnyMethod()));

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseCors();
app.MapControllers();

// Health check
app.MapGet("/api/health", () => new { status = "ok", time = DateTime.UtcNow });

Console.WriteLine("\n🌊 WordWave API đang chạy tại http://localhost:5000");
Console.WriteLine("   Swagger/test: http://localhost:5000/api/health\n");

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Run($"http://0.0.0.0:{port}");