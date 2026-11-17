using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordWave.Infrastructure.Migrations.Auth
{
    /// <inheritdoc />
    public partial class AddDailyLearning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_content_progress",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ContentId = table.Column<int>(type: "integer", nullable: false),
                    CorrectCount = table.Column<int>(type: "integer", nullable: false),
                    WrongCount = table.Column<int>(type: "integer", nullable: false),
                    NextReview = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastReviewed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_content_progress", x => new { x.UserId, x.ContentType, x.ContentId });
                    table.CheckConstraint("CK_user_content_progress_type", "\"ContentType\" IN ('grammar', 'patterns')");
                    table.ForeignKey(
                        name: "FK_user_content_progress_app_users_UserId",
                        column: x => x.UserId,
                        principalTable: "app_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_daily_lessons",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudyDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ContentId = table.Column<int>(type: "integer", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    IsNew = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Correct = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_daily_lessons", x => new { x.UserId, x.StudyDate, x.ContentType, x.ContentId });
                    table.CheckConstraint("CK_user_daily_lessons_type", "\"ContentType\" IN ('vocabulary', 'grammar', 'patterns')");
                    table.ForeignKey(
                        name: "FK_user_daily_lessons_app_users_UserId",
                        column: x => x.UserId,
                        principalTable: "app_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_study_activity",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudyDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    CorrectCount = table.Column<int>(type: "integer", nullable: false),
                    Xp = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_study_activity", x => new { x.UserId, x.StudyDate });
                    table.ForeignKey(
                        name: "FK_user_study_activity_app_users_UserId",
                        column: x => x.UserId,
                        principalTable: "app_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_content_progress");

            migrationBuilder.DropTable(
                name: "user_daily_lessons");

            migrationBuilder.DropTable(
                name: "user_study_activity");
        }
    }
}
