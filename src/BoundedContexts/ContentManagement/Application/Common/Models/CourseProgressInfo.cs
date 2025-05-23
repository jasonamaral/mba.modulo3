namespace FluencyHub.ContentManagement.Application.Common.Models;

public class CourseProgressInfo
{
    public Guid CourseId { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime LastUpdated { get; set; }
}