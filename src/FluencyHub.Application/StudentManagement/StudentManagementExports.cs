using FluencyHub.Application.Common.Exceptions;
using FluencyHub.StudentManagement.Application.Commands.ActivateStudent;
using FluencyHub.StudentManagement.Application.Commands.CompleteCourseForStudent;
using FluencyHub.StudentManagement.Application.Commands.CompleteLessonForStudent;
using FluencyHub.StudentManagement.Application.Commands.CreateStudent;
using FluencyHub.StudentManagement.Application.Commands.DeactivateStudent;
using FluencyHub.StudentManagement.Application.Commands.EnrollStudent;
using FluencyHub.StudentManagement.Application.Commands.UpdateStudent;
using FluencyHub.StudentManagement.Application.Queries.GetAllStudents;
using FluencyHub.StudentManagement.Application.Queries.GetCertificateById;
using FluencyHub.StudentManagement.Application.Queries.GetEnrollmentById;
using FluencyHub.StudentManagement.Application.Queries.GetStudentById;
using FluencyHub.StudentManagement.Application.Queries.GetStudentProgress;
using FluencyHub.Application.StudentManagement.Commands.GenerateCertificate;

namespace FluencyHub.Application.StudentManagement;

// Esta classe serve apenas para reexportar tipos do namespace FluencyHub.StudentManagement.Application
// para que eles possam ser usados através do namespace FluencyHub.Application.StudentManagement
public static class StudentManagementExports
{
    // Commands
    public record ActivateStudentCommand : FluencyHub.StudentManagement.Application.Commands.ActivateStudent.ActivateStudentCommand { }
    public record CompleteCourseForStudentCommand : FluencyHub.StudentManagement.Application.Commands.CompleteCourseForStudent.CompleteCourseForStudentCommand { }
    public class CompleteCourseForStudentResult : FluencyHub.StudentManagement.Application.Commands.CompleteCourseForStudent.CompleteCourseForStudentResult { }
    public record CompleteLessonForStudentCommand : FluencyHub.StudentManagement.Application.Commands.CompleteLessonForStudent.CompleteLessonForStudentCommand { }
    public class CompleteLessonForStudentResult : FluencyHub.StudentManagement.Application.Commands.CompleteLessonForStudent.CompleteLessonForStudentResult { }
    public record CreateStudentCommand : FluencyHub.StudentManagement.Application.Commands.CreateStudent.CreateStudentCommand { }
    public record DeactivateStudentCommand : FluencyHub.StudentManagement.Application.Commands.DeactivateStudent.DeactivateStudentCommand { }
    public record EnrollStudentCommand : FluencyHub.StudentManagement.Application.Commands.EnrollStudent.EnrollStudentCommand { }
    public record UpdateStudentCommand : FluencyHub.StudentManagement.Application.Commands.UpdateStudent.UpdateStudentCommand { }
    public record GenerateCertificateCommand : FluencyHub.Application.StudentManagement.Commands.GenerateCertificate.GenerateCertificateCommand { }

    // Queries and DTOs
    public class StudentDto : FluencyHub.StudentManagement.Application.Queries.GetStudentById.StudentDto { }
    public class StudentListDto : FluencyHub.StudentManagement.Application.Queries.GetAllStudents.StudentDto { }
    public class EnrollmentDto : FluencyHub.StudentManagement.Application.Queries.GetEnrollmentById.EnrollmentDto { }
    public class CertificateDto : FluencyHub.StudentManagement.Application.Queries.GetCertificateById.CertificateDto { }
    public class StudentProgressViewModel : FluencyHub.StudentManagement.Application.Queries.GetStudentProgress.StudentProgressViewModel { }

    // Command handlers and other types are automatically resolved through dependency injection
} 