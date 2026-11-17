using Microsoft.EntityFrameworkCore;
using WordWave.Domain.Models;

namespace WordWave.Infrastructure.Data;

// Separate migrations avoid depending on the incomplete legacy content baseline.
public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<UserSession> Sessions => Set<UserSession>();
    public DbSet<UserWordProgress> WordProgress => Set<UserWordProgress>();
    public DbSet<UserContentProgress> ContentProgress => Set<UserContentProgress>();
    public DbSet<UserDailyLesson> DailyLessons => Set<UserDailyLesson>();
    public DbSet<UserStudyActivity> StudyActivity => Set<UserStudyActivity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserContentProgress>(e =>
        {
            e.ToTable("user_content_progress", t => t.HasCheckConstraint("CK_user_content_progress_type", "\"ContentType\" IN ('grammar', 'patterns')"));
            e.HasKey(x => new { x.UserId, x.ContentType, x.ContentId });
            e.Property(x => x.ContentType).HasMaxLength(20);
            e.HasOne<AppUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<UserDailyLesson>(e =>
        {
            e.ToTable("user_daily_lessons", t => t.HasCheckConstraint("CK_user_daily_lessons_type", "\"ContentType\" IN ('vocabulary', 'grammar', 'patterns')"));
            e.HasKey(x => new { x.UserId, x.StudyDate, x.ContentType, x.ContentId });
            e.Property(x => x.ContentType).HasMaxLength(20);
            e.HasOne<AppUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<UserStudyActivity>(e =>
        {
            e.ToTable("user_study_activity");
            e.HasKey(x => new { x.UserId, x.StudyDate });
            e.HasOne<AppUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<UserWordProgress>(e =>
        {
            e.ToTable("user_word_progress");
            e.HasKey(x => new { x.UserId, x.WordId });
            e.HasOne<AppUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<AppUser>(e =>
        {
            e.ToTable("app_users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Email).HasMaxLength(254);
            e.Property(x => x.NormalizedEmail).HasMaxLength(254);
            e.Property(x => x.DisplayName).HasMaxLength(100);
            e.Property(x => x.Role).HasMaxLength(20);
            e.HasIndex(x => x.NormalizedEmail).IsUnique();
            e.ToTable(t => t.HasCheckConstraint("CK_app_users_role", "\"Role\" IN ('Admin', 'Learner')"));
        });
        modelBuilder.Entity<UserSession>(e =>
        {
            e.ToTable("user_sessions");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.ExpiresAt);
        });
    }
}
