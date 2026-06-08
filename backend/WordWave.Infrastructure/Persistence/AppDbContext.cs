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
    public DbSet<ToeicTest> ToeicTests { get; set; }
    public DbSet<ToeicPart> ToeicParts { get; set; }
    public DbSet<ToeicQuestion> ToeicQuestions { get; set; }
    public DbSet<ToeicAnswer> ToeicAnswers { get; set; }
    public DbSet<ToeicPassage> ToeicPassages { get; set; }
    public DbSet<ToeicAudio> ToeicAudios { get; set; }
    public DbSet<ToeicImportLog> ToeicImportLogs { get; set; }
    public DbSet<IeltsTest> IeltsTests { get; set; }
    public DbSet<IeltsAttempt> IeltsAttempts { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // Map tên bảng snake_case
        mb.Entity<VocabWord>().ToTable("vocabulary");
        mb.Entity<SentencePattern>().ToTable("sentence_patterns");
        mb.Entity<GrammarLesson>().ToTable("grammar_lessons");
        mb.Entity<GrammarExample>().ToTable("grammar_examples");
        mb.Entity<ToeicTest>().ToTable("toeic_tests");
        mb.Entity<ToeicPart>().ToTable("toeic_parts");
        mb.Entity<ToeicQuestion>().ToTable("toeic_questions");
        mb.Entity<ToeicAnswer>().ToTable("toeic_answers");
        mb.Entity<ToeicPassage>().ToTable("toeic_passages");
        mb.Entity<ToeicAudio>().ToTable("toeic_audios");
        mb.Entity<ToeicImportLog>().ToTable("toeic_import_logs");
        mb.Entity<IeltsTest>().ToTable("ielts_tests");
        mb.Entity<IeltsAttempt>().ToTable("ielts_attempts");

        // Map cột snake_case
        mb.Entity<VocabWord>(e => {
            e.Property(x => x.ExampleMeaning).HasColumnName("example_meaning");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
        });
        mb.Entity<SentencePattern>(e => {
            e.Property(x => x.Type).HasColumnName("type");
            e.Property(x => x.Examples).HasColumnType("text[]");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasIndex(x => x.Type);
        });
        mb.Entity<GrammarLesson>(e => {
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasMany(x => x.GrammarExamples)
                .WithOne()
                .HasForeignKey(x => x.LessonId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        mb.Entity<GrammarExample>(e => {
            e.Property(x => x.LessonId).HasColumnName("lesson_id");
        });
        mb.Entity<ToeicTest>(e => {
            e.Property(x => x.SourceType).HasColumnName("source_type");
            e.Property(x => x.SourceName).HasColumnName("source_name");
            e.Property(x => x.SourceUrl).HasColumnName("source_url");
            e.Property(x => x.ContentOwner).HasColumnName("content_owner");
            e.Property(x => x.IsPublic).HasColumnName("is_public");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasMany(x => x.Parts).WithOne(x => x.Test).HasForeignKey(x => x.ToeicTestId).OnDelete(DeleteBehavior.Cascade);
        });
        mb.Entity<ToeicPart>(e => {
            e.Property(x => x.ToeicTestId).HasColumnName("toeic_test_id");
            e.Property(x => x.PartNumber).HasColumnName("part_number");
            e.Property(x => x.OrderIndex).HasColumnName("order_index");
            e.HasMany(x => x.Questions).WithOne(x => x.Part).HasForeignKey(x => x.ToeicPartId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Passages).WithOne(x => x.Part).HasForeignKey(x => x.ToeicPartId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Audios).WithOne(x => x.Part).HasForeignKey(x => x.ToeicPartId).OnDelete(DeleteBehavior.Cascade);
        });
        mb.Entity<ToeicQuestion>(e => {
            e.Property(x => x.ToeicPartId).HasColumnName("toeic_part_id");
            e.Property(x => x.ToeicPassageId).HasColumnName("toeic_passage_id");
            e.Property(x => x.ToeicAudioId).HasColumnName("toeic_audio_id");
            e.Property(x => x.QuestionNumber).HasColumnName("question_number");
            e.Property(x => x.QuestionText).HasColumnName("question_text");
            e.Property(x => x.ImageUrl).HasColumnName("image_url");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasMany(x => x.Answers).WithOne(x => x.Question).HasForeignKey(x => x.ToeicQuestionId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Passage).WithMany(x => x.Questions).HasForeignKey(x => x.ToeicPassageId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Audio).WithMany(x => x.Questions).HasForeignKey(x => x.ToeicAudioId).OnDelete(DeleteBehavior.SetNull);
        });
        mb.Entity<ToeicAnswer>(e => {
            e.Property(x => x.ToeicQuestionId).HasColumnName("toeic_question_id");
            e.Property(x => x.AnswerText).HasColumnName("answer_text");
            e.Property(x => x.IsCorrect).HasColumnName("is_correct");
        });
        mb.Entity<ToeicPassage>(e => {
            e.Property(x => x.ToeicPartId).HasColumnName("toeic_part_id");
        });
        mb.Entity<ToeicAudio>(e => {
            e.Property(x => x.ToeicPartId).HasColumnName("toeic_part_id");
            e.Property(x => x.LocalPath).HasColumnName("local_path");
        });
        mb.Entity<ToeicImportLog>(e => {
            e.Property(x => x.SourceType).HasColumnName("source_type");
            e.Property(x => x.SourceName).HasColumnName("source_name");
            e.Property(x => x.SourceUrl).HasColumnName("source_url");
            e.Property(x => x.TotalItems).HasColumnName("total_items");
            e.Property(x => x.ImportedItems).HasColumnName("imported_items");
            e.Property(x => x.FailedItems).HasColumnName("failed_items");
            e.Property(x => x.ErrorMessage).HasColumnName("error_message");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
        });
        mb.Entity<IeltsTest>(e => {
            e.Property(x => x.SourceType).HasColumnName("source_type");
            e.Property(x => x.SourceName).HasColumnName("source_name");
            e.Property(x => x.TestData).HasColumnName("test_data").HasColumnType("jsonb");
            e.Property(x => x.QuestionCount).HasColumnName("question_count");
            e.Property(x => x.IsPublic).HasColumnName("is_public");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasMany(x => x.Attempts).WithOne(x => x.Test).HasForeignKey(x => x.IeltsTestId).OnDelete(DeleteBehavior.Cascade);
        });
        mb.Entity<IeltsAttempt>(e => {
            e.Property(x => x.IeltsTestId).HasColumnName("ielts_test_id");
            e.Property(x => x.LearnerId).HasColumnName("learner_id");
            e.Property(x => x.StateData).HasColumnName("state_data").HasColumnType("jsonb");
            e.Property(x => x.ResultData).HasColumnName("result_data").HasColumnType("jsonb");
            e.Property(x => x.IsSubmitted).HasColumnName("is_submitted");
            e.Property(x => x.OverallBand).HasColumnName("overall_band").HasPrecision(3, 1);
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.Property(x => x.SubmittedAt).HasColumnName("submitted_at");
            e.HasIndex(x => new { x.IeltsTestId, x.LearnerId }).IsUnique();
        });

        SeedToeicData(mb);
    }

    private static void SeedToeicData(ModelBuilder mb)
    {
        var createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        mb.Entity<ToeicTest>().HasData(new ToeicTest
        {
            Id = 1,
            Title = "WordWave TOEIC Mini Test",
            Description = "Original sample mini test covering TOEIC parts 1 to 7.",
            SourceType = "seed",
            SourceName = "WordWave original sample",
            SourceUrl = "",
            License = "Internal sample content; do not treat as official TOEIC material.",
            ContentOwner = "WordWave",
            IsPublic = true,
            CreatedAt = createdAt
        });

        mb.Entity<ToeicPart>().HasData(
            new ToeicPart { Id = 1, ToeicTestId = 1, PartNumber = 1, Name = "Picture Description", Instructions = "Choose the sentence that best describes the picture.", OrderIndex = 1 },
            new ToeicPart { Id = 2, ToeicTestId = 1, PartNumber = 2, Name = "Question Response", Instructions = "Choose the best response to each question.", OrderIndex = 2 },
            new ToeicPart { Id = 3, ToeicTestId = 1, PartNumber = 3, Name = "Conversations", Instructions = "Listen to each conversation and answer the questions.", OrderIndex = 3 },
            new ToeicPart { Id = 4, ToeicTestId = 1, PartNumber = 4, Name = "Talks", Instructions = "Listen to each talk and answer the questions.", OrderIndex = 4 },
            new ToeicPart { Id = 5, ToeicTestId = 1, PartNumber = 5, Name = "Incomplete Sentences", Instructions = "Choose the word or phrase that best completes the sentence.", OrderIndex = 5 },
            new ToeicPart { Id = 6, ToeicTestId = 1, PartNumber = 6, Name = "Text Completion", Instructions = "Choose the best answer to complete the text.", OrderIndex = 6 },
            new ToeicPart { Id = 7, ToeicTestId = 1, PartNumber = 7, Name = "Reading Comprehension", Instructions = "Read the text and answer the questions.", OrderIndex = 7 }
        );

        mb.Entity<ToeicAudio>().HasData(
            new ToeicAudio { Id = 1, ToeicPartId = 1, Url = "https://example.com/audio/toeic-part1-sample.mp3", Transcript = "A man is placing a laptop on a table." },
            new ToeicAudio { Id = 2, ToeicPartId = 2, Url = "https://example.com/audio/toeic-part2-sample.mp3", Transcript = "When will the shipment arrive?" },
            new ToeicAudio { Id = 3, ToeicPartId = 3, Url = "https://example.com/audio/toeic-part3-sample.mp3", Transcript = "Two coworkers discuss moving a client meeting to Thursday." },
            new ToeicAudio { Id = 4, ToeicPartId = 4, Url = "https://example.com/audio/toeic-part4-sample.mp3", Transcript = "A manager announces a change to the office schedule." }
        );

        mb.Entity<ToeicPassage>().HasData(
            new ToeicPassage { Id = 1, ToeicPartId = 6, Title = "Office Notice", Content = "Please remember that the monthly safety training will begin at 9 a.m. in Conference Room B. Employees should bring their ID cards." },
            new ToeicPassage { Id = 2, ToeicPartId = 7, Title = "Email from Facilities", Content = "The west entrance will be closed on Friday for maintenance. Staff may use the lobby entrance or the parking garage entrance." }
        );

        mb.Entity<ToeicQuestion>().HasData(
            new ToeicQuestion { Id = 1, ToeicPartId = 1, ToeicAudioId = 1, QuestionNumber = 1, Prompt = "Image: employee at desk", QuestionText = "What is happening in the picture?", ImageUrl = "https://example.com/images/toeic-part1-desk.jpg", Difficulty = "easy", CreatedAt = createdAt },
            new ToeicQuestion { Id = 2, ToeicPartId = 1, ToeicAudioId = 1, QuestionNumber = 2, Prompt = "Image: people near an elevator", QuestionText = "What does the picture show?", ImageUrl = "https://example.com/images/toeic-part1-elevator.jpg", Difficulty = "easy", CreatedAt = createdAt },
            new ToeicQuestion { Id = 3, ToeicPartId = 2, ToeicAudioId = 2, QuestionNumber = 3, Prompt = "When will the shipment arrive?", QuestionText = "Choose the best response.", Difficulty = "easy", CreatedAt = createdAt },
            new ToeicQuestion { Id = 4, ToeicPartId = 2, ToeicAudioId = 2, QuestionNumber = 4, Prompt = "Who reserved the conference room?", QuestionText = "Choose the best response.", Difficulty = "easy", CreatedAt = createdAt },
            new ToeicQuestion { Id = 5, ToeicPartId = 3, ToeicAudioId = 3, QuestionNumber = 5, Prompt = "Conversation about a meeting", QuestionText = "Why does the woman want to change the meeting time?", Difficulty = "medium", CreatedAt = createdAt },
            new ToeicQuestion { Id = 6, ToeicPartId = 3, ToeicAudioId = 3, QuestionNumber = 6, Prompt = "Conversation about a meeting", QuestionText = "What will the man probably do next?", Difficulty = "medium", CreatedAt = createdAt },
            new ToeicQuestion { Id = 7, ToeicPartId = 4, ToeicAudioId = 4, QuestionNumber = 7, Prompt = "Announcement", QuestionText = "What is the main purpose of the announcement?", Difficulty = "medium", CreatedAt = createdAt },
            new ToeicQuestion { Id = 8, ToeicPartId = 4, ToeicAudioId = 4, QuestionNumber = 8, Prompt = "Announcement", QuestionText = "Where should employees park tomorrow?", Difficulty = "medium", CreatedAt = createdAt },
            new ToeicQuestion { Id = 9, ToeicPartId = 5, QuestionNumber = 9, QuestionText = "The report must be submitted _____ Friday afternoon.", Difficulty = "easy", CreatedAt = createdAt },
            new ToeicQuestion { Id = 10, ToeicPartId = 5, QuestionNumber = 10, QuestionText = "Ms. Carter is responsible for _____ the new interns.", Difficulty = "easy", CreatedAt = createdAt },
            new ToeicQuestion { Id = 11, ToeicPartId = 6, ToeicPassageId = 1, QuestionNumber = 11, QuestionText = "Employees should bring their _____ to the training.", Difficulty = "easy", CreatedAt = createdAt },
            new ToeicQuestion { Id = 12, ToeicPartId = 6, ToeicPassageId = 1, QuestionNumber = 12, QuestionText = "The notice is mainly about _____.", Difficulty = "easy", CreatedAt = createdAt },
            new ToeicQuestion { Id = 13, ToeicPartId = 7, ToeicPassageId = 2, QuestionNumber = 13, QuestionText = "Why will the west entrance be closed?", Difficulty = "easy", CreatedAt = createdAt },
            new ToeicQuestion { Id = 14, ToeicPartId = 7, ToeicPassageId = 2, QuestionNumber = 14, QuestionText = "Which entrance is NOT mentioned as an alternative?", Difficulty = "easy", CreatedAt = createdAt }
        );

        mb.Entity<ToeicAnswer>().HasData(
            new ToeicAnswer { Id = 1, ToeicQuestionId = 1, Label = "A", AnswerText = "A man is placing a laptop on a table.", IsCorrect = true },
            new ToeicAnswer { Id = 2, ToeicQuestionId = 1, Label = "B", AnswerText = "A woman is watering plants.", IsCorrect = false },
            new ToeicAnswer { Id = 3, ToeicQuestionId = 1, Label = "C", AnswerText = "The chairs are being stacked.", IsCorrect = false },
            new ToeicAnswer { Id = 4, ToeicQuestionId = 1, Label = "D", AnswerText = "The lights are being repaired.", IsCorrect = false },
            new ToeicAnswer { Id = 5, ToeicQuestionId = 2, Label = "A", AnswerText = "People are waiting near an elevator.", IsCorrect = true },
            new ToeicAnswer { Id = 6, ToeicQuestionId = 2, Label = "B", AnswerText = "A truck is leaving a warehouse.", IsCorrect = false },
            new ToeicAnswer { Id = 7, ToeicQuestionId = 2, Label = "C", AnswerText = "A document is being printed.", IsCorrect = false },
            new ToeicAnswer { Id = 8, ToeicQuestionId = 2, Label = "D", AnswerText = "A meal is being served.", IsCorrect = false },
            new ToeicAnswer { Id = 9, ToeicQuestionId = 3, Label = "A", AnswerText = "It should be here by noon.", IsCorrect = true },
            new ToeicAnswer { Id = 10, ToeicQuestionId = 3, Label = "B", AnswerText = "At the loading dock.", IsCorrect = false },
            new ToeicAnswer { Id = 11, ToeicQuestionId = 3, Label = "C", AnswerText = "Because it was expensive.", IsCorrect = false },
            new ToeicAnswer { Id = 12, ToeicQuestionId = 4, Label = "A", AnswerText = "The marketing team did.", IsCorrect = true },
            new ToeicAnswer { Id = 13, ToeicQuestionId = 4, Label = "B", AnswerText = "For about thirty minutes.", IsCorrect = false },
            new ToeicAnswer { Id = 14, ToeicQuestionId = 4, Label = "C", AnswerText = "On the second floor.", IsCorrect = false },
            new ToeicAnswer { Id = 15, ToeicQuestionId = 5, Label = "A", AnswerText = "She has a client call at the original time.", IsCorrect = true },
            new ToeicAnswer { Id = 16, ToeicQuestionId = 5, Label = "B", AnswerText = "She lost the meeting notes.", IsCorrect = false },
            new ToeicAnswer { Id = 17, ToeicQuestionId = 5, Label = "C", AnswerText = "She is traveling to another city.", IsCorrect = false },
            new ToeicAnswer { Id = 18, ToeicQuestionId = 6, Label = "A", AnswerText = "Update the calendar invitation.", IsCorrect = true },
            new ToeicAnswer { Id = 19, ToeicQuestionId = 6, Label = "B", AnswerText = "Cancel the project.", IsCorrect = false },
            new ToeicAnswer { Id = 20, ToeicQuestionId = 6, Label = "C", AnswerText = "Print the sales report.", IsCorrect = false },
            new ToeicAnswer { Id = 21, ToeicQuestionId = 7, Label = "A", AnswerText = "To announce a schedule change.", IsCorrect = true },
            new ToeicAnswer { Id = 22, ToeicQuestionId = 7, Label = "B", AnswerText = "To introduce a new employee.", IsCorrect = false },
            new ToeicAnswer { Id = 23, ToeicQuestionId = 7, Label = "C", AnswerText = "To advertise a product.", IsCorrect = false },
            new ToeicAnswer { Id = 24, ToeicQuestionId = 8, Label = "A", AnswerText = "In the north lot.", IsCorrect = true },
            new ToeicAnswer { Id = 25, ToeicQuestionId = 8, Label = "B", AnswerText = "Beside the cafeteria.", IsCorrect = false },
            new ToeicAnswer { Id = 26, ToeicQuestionId = 8, Label = "C", AnswerText = "At the visitor desk.", IsCorrect = false },
            new ToeicAnswer { Id = 27, ToeicQuestionId = 9, Label = "A", AnswerText = "by", IsCorrect = true },
            new ToeicAnswer { Id = 28, ToeicQuestionId = 9, Label = "B", AnswerText = "among", IsCorrect = false },
            new ToeicAnswer { Id = 29, ToeicQuestionId = 9, Label = "C", AnswerText = "during", IsCorrect = false },
            new ToeicAnswer { Id = 30, ToeicQuestionId = 10, Label = "A", AnswerText = "training", IsCorrect = true },
            new ToeicAnswer { Id = 31, ToeicQuestionId = 10, Label = "B", AnswerText = "trained", IsCorrect = false },
            new ToeicAnswer { Id = 32, ToeicQuestionId = 10, Label = "C", AnswerText = "train", IsCorrect = false },
            new ToeicAnswer { Id = 33, ToeicQuestionId = 11, Label = "A", AnswerText = "ID cards", IsCorrect = true },
            new ToeicAnswer { Id = 34, ToeicQuestionId = 11, Label = "B", AnswerText = "laptops", IsCorrect = false },
            new ToeicAnswer { Id = 35, ToeicQuestionId = 11, Label = "C", AnswerText = "receipts", IsCorrect = false },
            new ToeicAnswer { Id = 36, ToeicQuestionId = 12, Label = "A", AnswerText = "a safety training session", IsCorrect = true },
            new ToeicAnswer { Id = 37, ToeicQuestionId = 12, Label = "B", AnswerText = "a company picnic", IsCorrect = false },
            new ToeicAnswer { Id = 38, ToeicQuestionId = 12, Label = "C", AnswerText = "a hiring announcement", IsCorrect = false },
            new ToeicAnswer { Id = 39, ToeicQuestionId = 13, Label = "A", AnswerText = "For maintenance.", IsCorrect = true },
            new ToeicAnswer { Id = 40, ToeicQuestionId = 13, Label = "B", AnswerText = "For a delivery.", IsCorrect = false },
            new ToeicAnswer { Id = 41, ToeicQuestionId = 13, Label = "C", AnswerText = "For a staff meeting.", IsCorrect = false },
            new ToeicAnswer { Id = 42, ToeicQuestionId = 14, Label = "A", AnswerText = "The cafeteria entrance.", IsCorrect = true },
            new ToeicAnswer { Id = 43, ToeicQuestionId = 14, Label = "B", AnswerText = "The lobby entrance.", IsCorrect = false },
            new ToeicAnswer { Id = 44, ToeicQuestionId = 14, Label = "C", AnswerText = "The parking garage entrance.", IsCorrect = false }
        );
    }
}
