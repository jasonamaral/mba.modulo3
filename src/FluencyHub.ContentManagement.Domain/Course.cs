using FluencyHub.ContentManagement.Domain.Common;
using System.Text.Json.Serialization;

namespace FluencyHub.ContentManagement.Domain;

public class Course : BaseEntity
{
    [JsonIgnore]
    private readonly List<Lesson> _lessons = [];

    public string Name { get; private set; }
    public string Description { get; private set; }
    public CourseContent Content { get; private set; }
    public decimal Price { get; private set; }
    public bool IsActive { get; private set; }
    public CourseStatus Status { get; private set; } = CourseStatus.Draft;
    public DateTime? PublishedAt { get; private set; }

    [JsonIgnore]
    public IReadOnlyCollection<Lesson> Lessons => _lessons.AsReadOnly();

    // EF Core constructor
    private Course()
    { }

    public Course(string name, string description, CourseContent content, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Course name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty", nameof(description));

        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));

        Name = name;
        Description = description;
        Content = content ?? throw new ArgumentException("Course content cannot be null", nameof(content));
        Price = price;
        IsActive = true;
        Status = CourseStatus.Draft;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string description, CourseContent content, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Course name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty", nameof(description));

        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));

        Name = name;
        Description = description;
        Content = content ?? throw new ArgumentException("Course content cannot be null", nameof(content));
        Price = price;
        UpdatedAt = DateTime.UtcNow;
    }

    public Lesson AddLesson(string title, string content, string? materialUrl = null)
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot add lessons to an inactive course");

        var order = _lessons.Count > 0 ? _lessons.Max(l => l.Order) + 1 : 1;
        var lesson = new Lesson(title, content, materialUrl, order);

        lesson.SetCourseId(this.Id);

        _lessons.Add(lesson);
        UpdatedAt = DateTime.UtcNow;

        return lesson;
    }

    public void UpdateLesson(Guid lessonId, string title, string content, string materialUrl)
    {
        var lesson = _lessons.FirstOrDefault(l => l.Id == lessonId) ?? throw new ArgumentException($"Lesson with ID {lessonId} not found", nameof(lessonId));
        lesson.Update(title, content, materialUrl);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveLesson(Guid lessonId)
    {
        var lesson = _lessons.FirstOrDefault(l => l.Id == lessonId) ?? throw new ArgumentException($"Lesson with ID {lessonId} not found", nameof(lessonId));
        _lessons.Remove(lesson);

        var orderedLessons = _lessons.OrderBy(l => l.Order).ToList();
        for (int i = 0; i < orderedLessons.Count; i++)
        {
            orderedLessons[i].UpdateOrder(i + 1);
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void ReorderLesson(Guid lessonId, int newOrder)
    {
        if (newOrder <= 0)
            throw new ArgumentException("Order must be positive", nameof(newOrder));

        var lesson = _lessons.FirstOrDefault(l => l.Id == lessonId) ?? throw new ArgumentException($"Lesson with ID {lessonId} not found", nameof(lessonId));
        var maxOrder = _lessons.Count;
        if (newOrder > maxOrder)
            newOrder = maxOrder;

        var currentOrder = lesson.Order;

        if (currentOrder == newOrder)
            return;

        foreach (var otherLesson in _lessons.Where(l => l.Id != lessonId))
        {
            if (currentOrder < newOrder) // Moving down
            {
                if (otherLesson.Order > currentOrder && otherLesson.Order <= newOrder)
                    otherLesson.UpdateOrder(otherLesson.Order - 1);
            }
            else // Moving up
            {
                if (otherLesson.Order >= newOrder && otherLesson.Order < currentOrder)
                    otherLesson.UpdateOrder(otherLesson.Order + 1);
            }
        }

        lesson.UpdateOrder(newOrder);
        UpdatedAt = DateTime.UtcNow;
    }

    public void PublishCourse()
    {
        if (Status == CourseStatus.Published)
            return;

        Status = CourseStatus.Published;
        PublishedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ArchiveCourse()
    {
        if (Status == CourseStatus.Archived)
            return;

        Status = CourseStatus.Archived;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
} 