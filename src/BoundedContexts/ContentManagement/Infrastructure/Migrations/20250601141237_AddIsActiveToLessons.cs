using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FluencyHub.ContentManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToLessons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Lessons",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Lessons");
        }
    }
}
