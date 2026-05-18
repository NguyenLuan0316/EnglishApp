using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WordWave.Infrastructure.Data;

#nullable disable

namespace WordWave.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260518050000_AddPatternType")]
    public partial class AddPatternType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "type",
                table: "sentence_patterns",
                type: "text",
                nullable: false,
                defaultValue: "daily");

            migrationBuilder.Sql(
                """
                WITH ranked_patterns AS (
                    SELECT id, ((ROW_NUMBER() OVER (ORDER BY id) - 1) / 20) + 1 AS group_number
                    FROM sentence_patterns
                )
                UPDATE sentence_patterns
                SET type = CASE
                    WHEN ranked_patterns.group_number = 1 THEN 'greetings'
                    WHEN ranked_patterns.group_number = 2 THEN 'small-talk'
                    WHEN ranked_patterns.group_number = 3 THEN 'introductions'
                    WHEN ranked_patterns.group_number = 4 THEN 'requests'
                    WHEN ranked_patterns.group_number = 5 THEN 'clarification'
                    WHEN ranked_patterns.group_number = 6 THEN 'confirmation'
                    WHEN ranked_patterns.group_number = 7 THEN 'opinions'
                    WHEN ranked_patterns.group_number = 8 THEN 'agreement'
                    WHEN ranked_patterns.group_number = 9 THEN 'disagreement'
                    WHEN ranked_patterns.group_number = 10 THEN 'suggestions'
                    WHEN ranked_patterns.group_number = 11 THEN 'invitations'
                    WHEN ranked_patterns.group_number = 12 THEN 'planning'
                    WHEN ranked_patterns.group_number = 13 THEN 'scheduling'
                    WHEN ranked_patterns.group_number = 14 THEN 'phone-messaging'
                    WHEN ranked_patterns.group_number = 15 THEN 'dining'
                    WHEN ranked_patterns.group_number = 16 THEN 'shopping'
                    WHEN ranked_patterns.group_number = 17 THEN 'travel'
                    WHEN ranked_patterns.group_number = 18 THEN 'work'
                    WHEN ranked_patterns.group_number = 19 THEN 'problem-solving'
                    WHEN ranked_patterns.group_number = 20 THEN 'apologies'
                    WHEN ranked_patterns.group_number = 21 THEN 'thanks'
                    WHEN ranked_patterns.group_number = 22 THEN 'feelings'
                    WHEN ranked_patterns.group_number = 23 THEN 'health'
                    WHEN ranked_patterns.group_number = 24 THEN 'english-learning'
                    WHEN ranked_patterns.group_number = 25 THEN 'closings'
                    ELSE type
                END
                FROM ranked_patterns
                WHERE sentence_patterns.id = ranked_patterns.id;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_sentence_patterns_type",
                table: "sentence_patterns",
                column: "type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_sentence_patterns_type",
                table: "sentence_patterns");

            migrationBuilder.DropColumn(
                name: "type",
                table: "sentence_patterns");
        }
    }
}
