using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FluencyHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Ajustes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LearningRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LessonId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Grade = table.Column<float>(type: "REAL", nullable: true),
                    LearningHistoryId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningRecords_LearningHistories_LearningHistoryId",
                        column: x => x.LearningHistoryId,
                        principalTable: "LearningHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LearningRecords_LearningHistoryId",
                table: "LearningRecords",
                column: "LearningHistoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LearningRecords");
        }
    }
}
