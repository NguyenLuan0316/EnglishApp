using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814

namespace WordWave.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddToeicDataImport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "toeic_import_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    source_type = table.Column<string>(type: "text", nullable: false),
                    source_name = table.Column<string>(type: "text", nullable: false),
                    source_url = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    total_items = table.Column<int>(type: "integer", nullable: false),
                    imported_items = table.Column<int>(type: "integer", nullable: false),
                    failed_items = table.Column<int>(type: "integer", nullable: false),
                    error_message = table.Column<string>(type: "text", nullable: false),
                    details = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_toeic_import_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "toeic_tests",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    source_type = table.Column<string>(type: "text", nullable: false),
                    source_name = table.Column<string>(type: "text", nullable: false),
                    source_url = table.Column<string>(type: "text", nullable: false),
                    license = table.Column<string>(type: "text", nullable: false),
                    content_owner = table.Column<string>(type: "text", nullable: false),
                    is_public = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_toeic_tests", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "toeic_parts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    toeic_test_id = table.Column<int>(type: "integer", nullable: false),
                    part_number = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    instructions = table.Column<string>(type: "text", nullable: false),
                    order_index = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_toeic_parts", x => x.id);
                    table.ForeignKey(
                        name: "FK_toeic_parts_toeic_tests_toeic_test_id",
                        column: x => x.toeic_test_id,
                        principalTable: "toeic_tests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "toeic_audios",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    toeic_part_id = table.Column<int>(type: "integer", nullable: false),
                    url = table.Column<string>(type: "text", nullable: false),
                    local_path = table.Column<string>(type: "text", nullable: false),
                    transcript = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_toeic_audios", x => x.id);
                    table.ForeignKey(
                        name: "FK_toeic_audios_toeic_parts_toeic_part_id",
                        column: x => x.toeic_part_id,
                        principalTable: "toeic_parts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "toeic_passages",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    toeic_part_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_toeic_passages", x => x.id);
                    table.ForeignKey(
                        name: "FK_toeic_passages_toeic_parts_toeic_part_id",
                        column: x => x.toeic_part_id,
                        principalTable: "toeic_parts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "toeic_questions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    toeic_part_id = table.Column<int>(type: "integer", nullable: false),
                    toeic_passage_id = table.Column<int>(type: "integer", nullable: true),
                    toeic_audio_id = table.Column<int>(type: "integer", nullable: true),
                    question_number = table.Column<int>(type: "integer", nullable: false),
                    prompt = table.Column<string>(type: "text", nullable: false),
                    question_text = table.Column<string>(type: "text", nullable: false),
                    image_url = table.Column<string>(type: "text", nullable: false),
                    difficulty = table.Column<string>(type: "text", nullable: false),
                    explanation = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_toeic_questions", x => x.id);
                    table.ForeignKey(
                        name: "FK_toeic_questions_toeic_audios_toeic_audio_id",
                        column: x => x.toeic_audio_id,
                        principalTable: "toeic_audios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_toeic_questions_toeic_parts_toeic_part_id",
                        column: x => x.toeic_part_id,
                        principalTable: "toeic_parts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_toeic_questions_toeic_passages_toeic_passage_id",
                        column: x => x.toeic_passage_id,
                        principalTable: "toeic_passages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "toeic_answers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    toeic_question_id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "text", nullable: false),
                    answer_text = table.Column<string>(type: "text", nullable: false),
                    is_correct = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_toeic_answers", x => x.id);
                    table.ForeignKey(
                        name: "FK_toeic_answers_toeic_questions_toeic_question_id",
                        column: x => x.toeic_question_id,
                        principalTable: "toeic_questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "toeic_tests",
                columns: new[] { "id", "content_owner", "created_at", "description", "is_public", "license", "source_name", "source_type", "source_url", "title" },
                values: new object[] { 1, "WordWave", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Original sample mini test covering TOEIC parts 1 to 7.", true, "Internal sample content; do not treat as official TOEIC material.", "WordWave original sample", "seed", "", "WordWave TOEIC Mini Test" });

            migrationBuilder.InsertData(
                table: "toeic_parts",
                columns: new[] { "id", "instructions", "name", "order_index", "part_number", "toeic_test_id" },
                values: new object[,]
                {
                    { 1, "Choose the sentence that best describes the picture.", "Picture Description", 1, 1, 1 },
                    { 2, "Choose the best response to each question.", "Question Response", 2, 2, 1 },
                    { 3, "Listen to each conversation and answer the questions.", "Conversations", 3, 3, 1 },
                    { 4, "Listen to each talk and answer the questions.", "Talks", 4, 4, 1 },
                    { 5, "Choose the word or phrase that best completes the sentence.", "Incomplete Sentences", 5, 5, 1 },
                    { 6, "Choose the best answer to complete the text.", "Text Completion", 6, 6, 1 },
                    { 7, "Read the text and answer the questions.", "Reading Comprehension", 7, 7, 1 }
                });

            migrationBuilder.InsertData(
                table: "toeic_audios",
                columns: new[] { "id", "local_path", "toeic_part_id", "transcript", "url" },
                values: new object[,]
                {
                    { 1, "", 1, "A man is placing a laptop on a table.", "https://example.com/audio/toeic-part1-sample.mp3" },
                    { 2, "", 2, "When will the shipment arrive?", "https://example.com/audio/toeic-part2-sample.mp3" },
                    { 3, "", 3, "Two coworkers discuss moving a client meeting to Thursday.", "https://example.com/audio/toeic-part3-sample.mp3" },
                    { 4, "", 4, "A manager announces a change to the office schedule.", "https://example.com/audio/toeic-part4-sample.mp3" }
                });

            migrationBuilder.InsertData(
                table: "toeic_passages",
                columns: new[] { "id", "content", "title", "toeic_part_id" },
                values: new object[,]
                {
                    { 1, "Please remember that the monthly safety training will begin at 9 a.m. in Conference Room B. Employees should bring their ID cards.", "Office Notice", 6 },
                    { 2, "The west entrance will be closed on Friday for maintenance. Staff may use the lobby entrance or the parking garage entrance.", "Email from Facilities", 7 }
                });

            migrationBuilder.InsertData(
                table: "toeic_questions",
                columns: new[] { "id", "created_at", "difficulty", "explanation", "image_url", "prompt", "question_number", "question_text", "toeic_audio_id", "toeic_part_id", "toeic_passage_id" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "easy", "", "https://example.com/images/toeic-part1-desk.jpg", "Image: employee at desk", 1, "What is happening in the picture?", 1, 1, null },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "easy", "", "https://example.com/images/toeic-part1-elevator.jpg", "Image: people near an elevator", 2, "What does the picture show?", 1, 1, null },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "easy", "", "", "When will the shipment arrive?", 3, "Choose the best response.", 2, 2, null },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "easy", "", "", "Who reserved the conference room?", 4, "Choose the best response.", 2, 2, null },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "medium", "", "", "Conversation about a meeting", 5, "Why does the woman want to change the meeting time?", 3, 3, null },
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "medium", "", "", "Conversation about a meeting", 6, "What will the man probably do next?", 3, 3, null },
                    { 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "medium", "", "", "Announcement", 7, "What is the main purpose of the announcement?", 4, 4, null },
                    { 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "medium", "", "", "Announcement", 8, "Where should employees park tomorrow?", 4, 4, null },
                    { 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "easy", "", "", "", 9, "The report must be submitted _____ Friday afternoon.", null, 5, null },
                    { 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "easy", "", "", "", 10, "Ms. Carter is responsible for _____ the new interns.", null, 5, null },
                    { 11, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "easy", "", "", "", 11, "Employees should bring their _____ to the training.", null, 6, 1 },
                    { 12, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "easy", "", "", "", 12, "The notice is mainly about _____.", null, 6, 1 },
                    { 13, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "easy", "", "", "", 13, "Why will the west entrance be closed?", null, 7, 2 },
                    { 14, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "easy", "", "", "", 14, "Which entrance is NOT mentioned as an alternative?", null, 7, 2 }
                });

            migrationBuilder.InsertData(
                table: "toeic_answers",
                columns: new[] { "id", "answer_text", "is_correct", "label", "toeic_question_id" },
                values: new object[,]
                {
                    { 1, "A man is placing a laptop on a table.", true, "A", 1 },
                    { 2, "A woman is watering plants.", false, "B", 1 },
                    { 3, "The chairs are being stacked.", false, "C", 1 },
                    { 4, "The lights are being repaired.", false, "D", 1 },
                    { 5, "People are waiting near an elevator.", true, "A", 2 },
                    { 6, "A truck is leaving a warehouse.", false, "B", 2 },
                    { 7, "A document is being printed.", false, "C", 2 },
                    { 8, "A meal is being served.", false, "D", 2 },
                    { 9, "It should be here by noon.", true, "A", 3 },
                    { 10, "At the loading dock.", false, "B", 3 },
                    { 11, "Because it was expensive.", false, "C", 3 },
                    { 12, "The marketing team did.", true, "A", 4 },
                    { 13, "For about thirty minutes.", false, "B", 4 },
                    { 14, "On the second floor.", false, "C", 4 },
                    { 15, "She has a client call at the original time.", true, "A", 5 },
                    { 16, "She lost the meeting notes.", false, "B", 5 },
                    { 17, "She is traveling to another city.", false, "C", 5 },
                    { 18, "Update the calendar invitation.", true, "A", 6 },
                    { 19, "Cancel the project.", false, "B", 6 },
                    { 20, "Print the sales report.", false, "C", 6 },
                    { 21, "To announce a schedule change.", true, "A", 7 },
                    { 22, "To introduce a new employee.", false, "B", 7 },
                    { 23, "To advertise a product.", false, "C", 7 },
                    { 24, "In the north lot.", true, "A", 8 },
                    { 25, "Beside the cafeteria.", false, "B", 8 },
                    { 26, "At the visitor desk.", false, "C", 8 },
                    { 27, "by", true, "A", 9 },
                    { 28, "among", false, "B", 9 },
                    { 29, "during", false, "C", 9 },
                    { 30, "training", true, "A", 10 },
                    { 31, "trained", false, "B", 10 },
                    { 32, "train", false, "C", 10 },
                    { 33, "ID cards", true, "A", 11 },
                    { 34, "laptops", false, "B", 11 },
                    { 35, "receipts", false, "C", 11 },
                    { 36, "a safety training session", true, "A", 12 },
                    { 37, "a company picnic", false, "B", 12 },
                    { 38, "a hiring announcement", false, "C", 12 },
                    { 39, "For maintenance.", true, "A", 13 },
                    { 40, "For a delivery.", false, "B", 13 },
                    { 41, "For a staff meeting.", false, "C", 13 },
                    { 42, "The cafeteria entrance.", true, "A", 14 },
                    { 43, "The lobby entrance.", false, "B", 14 },
                    { 44, "The parking garage entrance.", false, "C", 14 }
                });

            migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('toeic_tests', 'id'), COALESCE((SELECT MAX(id) FROM toeic_tests), 1));");
            migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('toeic_parts', 'id'), COALESCE((SELECT MAX(id) FROM toeic_parts), 1));");
            migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('toeic_audios', 'id'), COALESCE((SELECT MAX(id) FROM toeic_audios), 1));");
            migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('toeic_passages', 'id'), COALESCE((SELECT MAX(id) FROM toeic_passages), 1));");
            migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('toeic_questions', 'id'), COALESCE((SELECT MAX(id) FROM toeic_questions), 1));");
            migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('toeic_answers', 'id'), COALESCE((SELECT MAX(id) FROM toeic_answers), 1));");

            migrationBuilder.CreateIndex(name: "IX_toeic_answers_toeic_question_id", table: "toeic_answers", column: "toeic_question_id");
            migrationBuilder.CreateIndex(name: "IX_toeic_audios_toeic_part_id", table: "toeic_audios", column: "toeic_part_id");
            migrationBuilder.CreateIndex(name: "IX_toeic_parts_toeic_test_id", table: "toeic_parts", column: "toeic_test_id");
            migrationBuilder.CreateIndex(name: "IX_toeic_passages_toeic_part_id", table: "toeic_passages", column: "toeic_part_id");
            migrationBuilder.CreateIndex(name: "IX_toeic_questions_toeic_audio_id", table: "toeic_questions", column: "toeic_audio_id");
            migrationBuilder.CreateIndex(name: "IX_toeic_questions_toeic_part_id", table: "toeic_questions", column: "toeic_part_id");
            migrationBuilder.CreateIndex(name: "IX_toeic_questions_toeic_passage_id", table: "toeic_questions", column: "toeic_passage_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "toeic_answers");
            migrationBuilder.DropTable(name: "toeic_import_logs");
            migrationBuilder.DropTable(name: "toeic_questions");
            migrationBuilder.DropTable(name: "toeic_audios");
            migrationBuilder.DropTable(name: "toeic_passages");
            migrationBuilder.DropTable(name: "toeic_parts");
            migrationBuilder.DropTable(name: "toeic_tests");
        }
    }
}
