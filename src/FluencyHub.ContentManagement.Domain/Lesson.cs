using FluencyHub.Common.Domain;
using System.Text.Json.Serialization;

namespace FluencyHub.ContentManagement.Domain;

public class Lesson : BaseEntity
{
    public Guid CourseId { get; private set; }
    public string Title { get; private set; }
    public string Content { get; private set; }
    public string? MaterialUrl { get; private set; }
    public int Order { get; private set; }

    // Navegação para EF Core
    [JsonIgnore]
    public Course Course { get; private set; }

    // EF Core constructor
    private Lesson()
    { }

    public Lesson(string title, string content, string? materialUrl, int order)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Lesson title cannot be empty", nameof(title));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Lesson content cannot be empty", nameof(content));

        if (order <= 0)
            throw new ArgumentException("Order must be positive", nameof(order));

        Title = title;
        Content = content;
        MaterialUrl = materialUrl;
        Order = order;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string title, string content, string? materialUrl)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Lesson title cannot be empty", nameof(title));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Lesson content cannot be empty", nameof(content));

        Title = title;
        Content = content;
        MaterialUrl = materialUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateOrder(int newOrder)
    {
        if (newOrder <= 0)
            throw new ArgumentException("Order must be positive", nameof(newOrder));

        Order = newOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateMaterial(string materialUrl)
    {
        MaterialUrl = materialUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    internal void SetCourseId(Guid courseId)
    {
        CourseId = courseId;
    }
} 