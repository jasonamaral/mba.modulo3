using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FluencyHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialSQLiteMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardDetails_CardHolderName",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CardDetails_ExpiryMonth",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CardDetails_ExpiryYear",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CardDetails_MaskedCardNumber",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Content_Language",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Content_LearningObjectives",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Content_Level",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Content_PreRequisites",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Content_Syllabus",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Content_TargetAudience",
                table: "Courses");

            migrationBuilder.AlterColumn<Guid>(
                name: "CourseId",
                table: "Lessons",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentId",
                table: "Enrollments",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Courses",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentId",
                table: "Certificates",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "CardDetails",
                columns: table => new
                {
                    PaymentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CardHolderName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MaskedCardNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ExpiryMonth = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    ExpiryYear = table.Column<string>(type: "TEXT", maxLength: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardDetails", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_CardDetails_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseContents",
                columns: table => new
                {
                    CourseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Syllabus = table.Column<string>(type: "TEXT", maxLength: 5000, nullable: false),
                    LearningObjectives = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    PreRequisites = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    TargetAudience = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Language = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Level = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseContents", x => x.CourseId);
                    table.ForeignKey(
                        name: "FK_CourseContents_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_EnrollmentId",
                table: "Payments",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_CourseId",
                table: "Enrollments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_CourseId",
                table: "Certificates",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificates_Courses_CourseId",
                table: "Certificates",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Courses_CourseId",
                table: "Enrollments",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Enrollments_EnrollmentId",
                table: "Payments",
                column: "EnrollmentId",
                principalTable: "Enrollments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Certificates_Courses_CourseId",
                table: "Certificates");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Courses_CourseId",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Enrollments_EnrollmentId",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "CardDetails");

            migrationBuilder.DropTable(
                name: "CourseContents");

            migrationBuilder.DropIndex(
                name: "IX_Payments_EnrollmentId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_CourseId",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_Certificates_CourseId",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Courses");

            migrationBuilder.AddColumn<string>(
                name: "CardDetails_CardHolderName",
                table: "Payments",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CardDetails_ExpiryMonth",
                table: "Payments",
                type: "TEXT",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CardDetails_ExpiryYear",
                table: "Payments",
                type: "TEXT",
                maxLength: 4,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CardDetails_MaskedCardNumber",
                table: "Payments",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "CourseId",
                table: "Lessons",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentId",
                table: "Enrollments",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "Content_Language",
                table: "Courses",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Content_LearningObjectives",
                table: "Courses",
                type: "TEXT",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Content_Level",
                table: "Courses",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Content_PreRequisites",
                table: "Courses",
                type: "TEXT",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Content_Syllabus",
                table: "Courses",
                type: "TEXT",
                maxLength: 5000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Content_TargetAudience",
                table: "Courses",
                type: "TEXT",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentId",
                table: "Certificates",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");
        }
    }
}
