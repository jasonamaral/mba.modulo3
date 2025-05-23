namespace FluencyHub.ContentManagement.Application.Queries.GetCourseById;

public class CourseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Syllabus { get; set; } = string.Empty;
    public string LearningObjectives { get; set; } = string.Empty;
    public string PreRequisites { get; set; } = string.Empty;
    public string TargetAudience { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public decimal Price { get; set; }
} 