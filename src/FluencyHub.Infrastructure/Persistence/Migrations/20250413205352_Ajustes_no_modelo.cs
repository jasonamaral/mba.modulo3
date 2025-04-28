using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FluencyHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Ajustes_no_modelo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FailureReason",
                table: "Payments",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefundReason",
                table: "Payments",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "Courses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Courses",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "LearningHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CourseProgresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CourseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LearningHistoryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseProgresses_LearningHistories_LearningHistoryId",
                        column: x => x.LearningHistoryId,
                        principalTable: "LearningHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompletedLessons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LessonId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CourseProgressId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletedLessons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompletedLessons_CourseProgresses_CourseProgressId",
                        column: x => x.CourseProgressId,
                        principalTable: "CourseProgresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompletedLessons_CourseProgressId",
                table: "CompletedLessons",
                column: "CourseProgressId");

            migrationBuilder.CreateIndex(
                name: "IX_CompletedLessons_CourseProgressId_LessonId",
                table: "CompletedLessons",
                columns: new[] { "CourseProgressId", "LessonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseProgresses_LearningHistoryId",
                table: "CourseProgresses",
                column: "LearningHistoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompletedLessons");

            migrationBuilder.DropTable(
                name: "CourseProgresses");

            migrationBuilder.DropTable(
                name: "LearningHistories");

            migrationBuilder.DropColumn(
                name: "FailureReason",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RefundReason",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Courses");
        }
    }
}
