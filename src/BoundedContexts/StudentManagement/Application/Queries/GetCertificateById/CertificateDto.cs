namespace FluencyHub.StudentManagement.Application.Queries.GetCertificateById;

public class CertificateDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public int? Score { get; set; }
    public string? Feedback { get; set; }
} 