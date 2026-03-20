using Microsoft.EntityFrameworkCore;
using WordWave.Domain.Models;

namespace WordWave.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<VocabWord> Vocabulary { get; set; }
    public DbSet<SentencePattern> SentencePatterns { get; set; }
    public DbSet<GrammarLesson> GrammarLessons { get; set; }
    public DbSet<GrammarExample> GrammarExamples { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // Map tên bảng snake_case
        mb.Entity<VocabWord>().ToTable("vocabulary");
        mb.Entity<SentencePattern>().ToTable("sentence_patterns");
        mb.Entity<GrammarLesson>().ToTable("grammar_lessons");
        mb.Entity<GrammarExample>().ToTable("grammar_examples");

        // Map cột snake_case
        mb.Entity<VocabWord>(e => {
            e.Property(x => x.ExampleMeaning).HasColumnName("example_meaning");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
        });
        mb.Entity<SentencePattern>(e => {
            e.Property(x => x.Examples).HasColumnType("text[]");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
        });
        mb.Entity<GrammarLesson>(e => {
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
        });
        mb.Entity<GrammarExample>(e => {
            e.Property(x => x.LessonId).HasColumnName("lesson_id");
        });
    }
}