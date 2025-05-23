namespace FluencyHub.StudentManagement.Application.Commands.CompleteCourseForStudent;

public class CompleteCourseForStudentResult
{
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public bool IsSuccessful { get; set; }
    public int? FinalScore { get; set; }
    public bool CertificateGenerated { get; set; }
    public Guid? CertificateId { get; set; }
} 