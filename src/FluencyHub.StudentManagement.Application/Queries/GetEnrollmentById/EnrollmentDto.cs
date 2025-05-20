namespace FluencyHub.StudentManagement.Application.Queries.GetEnrollmentById;

public class EnrollmentDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal FinalPrice { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletionDate { get; set; }
    public int? Progress { get; set; }
} 