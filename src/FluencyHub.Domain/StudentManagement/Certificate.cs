using FluencyHub.Domain.Common;
using FluencyHub.Domain.ContentManagement;
using System.Text.Json.Serialization;

namespace FluencyHub.Domain.StudentManagement;

public class Certificate : BaseEntity
{
    public Guid StudentId { get; private set; }
    public Guid CourseId { get; private set; }
    public string Title { get; private set; }
    public DateTime IssueDate { get; private set; }

    [JsonIgnore]
    public Student Student { get; private set; }

    [JsonIgnore]
    public Course Course { get; private set; }

    private Certificate()
    { }

    public Certificate(Guid studentId, Guid courseId, string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Certificate title cannot be empty", nameof(title));

        StudentId = studentId;
        CourseId = courseId;
        Title = title;
        IssueDate = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Certificate title cannot be empty", nameof(title));

        Title = title;
        UpdatedAt = DateTime.UtcNow;
    }
}