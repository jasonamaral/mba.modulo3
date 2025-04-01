using FluencyHub.Domain.Common;
using System.Text.Json.Serialization;

namespace FluencyHub.Domain.ContentManagement;

public class Course : BaseEntity
{
    [JsonIgnore]
    private readonly List<Lesson> _lessons = new();

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

        if (content == null)
            throw new ArgumentException("Course content cannot be null", nameof(content));

        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));

        Name = name;
        Description = description;
        Content = content;
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

        if (content == null)
            throw new ArgumentException("Course content cannot be null", nameof(content));

        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));

        Name = name;
        Description = description;
        Content = content;
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
        var lesson = _lessons.FirstOrDefault(l => l.Id == lessonId);

        if (lesson == null)
            throw new ArgumentException($"Lesson with ID {lessonId} not found", nameof(lessonId));

        lesson.Update(title, content, materialUrl);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveLesson(Guid lessonId)
    {
        var lesson = _lessons.FirstOrDefault(l => l.Id == lessonId);

        if (lesson == null)
            throw new ArgumentException($"Lesson with ID {lessonId} not found", nameof(lessonId));

        _lessons.Remove(lesson);

        // Resequence remaining lessons to maintain continuous ordering
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

        var lesson = _lessons.FirstOrDefault(l => l.Id == lessonId);

        if (lesson == null)
            throw new ArgumentException($"Lesson with ID {lessonId} not found", nameof(lessonId));

        var maxOrder = _lessons.Count;
        if (newOrder > maxOrder)
            newOrder = maxOrder;

        var currentOrder = lesson.Order;

        // Skip if the lesson is already in the requested position
        if (currentOrder == newOrder)
            return;

        // Shift lessons between old and new positions
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

        // Update the target lesson order
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