using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using WordWave.Infrastructure.Data;

#nullable disable

namespace WordWave.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260518100000_AddIeltsTests")]
    public partial class AddIeltsTests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ielts_tests",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    source_type = table.Column<string>(type: "text", nullable: false),
                    source_name = table.Column<string>(type: "text", nullable: false),
                    test_data = table.Column<string>(type: "jsonb", nullable: false),
                    question_count = table.Column<int>(type: "integer", nullable: false),
                    is_public = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ielts_tests", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ielts_attempts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ielts_test_id = table.Column<int>(type: "integer", nullable: false),
                    learner_id = table.Column<string>(type: "text", nullable: false),
                    state_data = table.Column<string>(type: "jsonb", nullable: false),
                    result_data = table.Column<string>(type: "jsonb", nullable: false),
                    is_submitted = table.Column<bool>(type: "boolean", nullable: false),
                    overall_band = table.Column<decimal>(type: "numeric(3,1)", precision: 3, scale: 1, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ielts_attempts", x => x.id);
                    table.ForeignKey(
                        name: "FK_ielts_attempts_ielts_tests_ielts_test_id",
                        column: x => x.ielts_test_id,
                        principalTable: "ielts_tests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ielts_attempts_ielts_test_id_learner_id",
                table: "ielts_attempts",
                columns: new[] { "ielts_test_id", "learner_id" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ielts_attempts");
            migrationBuilder.DropTable(name: "ielts_tests");
        }
    }
}
