using FluencyHub.Domain.Common;
using System.Text.Json.Serialization;

namespace FluencyHub.Domain.StudentManagement;

public class LearningHistory : BaseEntity
{
    private readonly List<CourseProgress> _courseProgress = new();
    
    public IReadOnlyCollection<CourseProgress> CourseProgress => _courseProgress;
    
    // Construtor para o EF Core
    protected LearningHistory() { }
    
    public LearningHistory(Guid studentId)
    {
        Id = studentId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void AddProgress(Guid courseId, Guid lessonId)
    {
        var courseProgress = _courseProgress.FirstOrDefault(cp => cp.CourseId == courseId);
        if (courseProgress == null)
        {
            courseProgress = new CourseProgress(courseId);
            _courseProgress.Add(courseProgress);
        }
        
        courseProgress.AddCompletedLesson(lessonId);
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void CompleteCourse(Guid courseId)
    {
        var courseProgress = _courseProgress.FirstOrDefault(cp => cp.CourseId == courseId);
        if (courseProgress == null)
        {
            courseProgress = new CourseProgress(courseId);
            _courseProgress.Add(courseProgress);
        }
        
        courseProgress.CompleteCourse();
        UpdatedAt = DateTime.UtcNow;
    }
    
    public bool HasCompletedLesson(Guid courseId, Guid lessonId)
    {
        var courseProgress = _courseProgress.FirstOrDefault(cp => cp.CourseId == courseId);
        return courseProgress != null && courseProgress.CompletedLessons.Contains(lessonId);
    }
    
    public bool HasCompletedCourse(Guid courseId)
    {
        var courseProgress = _courseProgress.FirstOrDefault(cp => cp.CourseId == courseId);
        return courseProgress != null && courseProgress.IsCompleted;
    }
    
    public int GetCompletedLessonsCount(Guid courseId)
    {
        var courseProgress = _courseProgress.FirstOrDefault(cp => cp.CourseId == courseId);
        return courseProgress != null ? courseProgress.CompletedLessons.Count : 0;
    }
}

public class CourseProgress
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    private readonly HashSet<Guid> _completedLessons = new();
    
    public Guid CourseId { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTime LastUpdated { get; private set; }
    
    [JsonIgnore]
    public IReadOnlyCollection<Guid> CompletedLessons => _completedLessons;
    
    public CourseProgress(Guid courseId)
    {
        CourseId = courseId;
        IsCompleted = false;
        LastUpdated = DateTime.UtcNow;
    }
    
    public void AddCompletedLesson(Guid lessonId)
    {
        _completedLessons.Add(lessonId);
        LastUpdated = DateTime.UtcNow;
    }
    
    public void CompleteCourse()
    {
        IsCompleted = true;
        LastUpdated = DateTime.UtcNow;
    }
} 