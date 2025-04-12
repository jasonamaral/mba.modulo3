using FluencyHub.Domain.Common;
using System.Text.Json.Serialization;

namespace FluencyHub.Domain.StudentManagement;

public class LearningHistory : BaseEntity
{
    private readonly Dictionary<Guid, CourseProgress> _courseProgress = new();
    
    public IReadOnlyDictionary<Guid, CourseProgress> CourseProgress => _courseProgress;
    
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
        if (!_courseProgress.ContainsKey(courseId))
        {
            _courseProgress[courseId] = new CourseProgress(courseId);
        }
        
        _courseProgress[courseId].AddCompletedLesson(lessonId);
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void CompleteCourse(Guid courseId)
    {
        if (!_courseProgress.ContainsKey(courseId))
        {
            _courseProgress[courseId] = new CourseProgress(courseId);
        }
        
        _courseProgress[courseId].CompleteCourse();
        UpdatedAt = DateTime.UtcNow;
    }
    
    public bool HasCompletedLesson(Guid courseId, Guid lessonId)
    {
        return _courseProgress.ContainsKey(courseId) && 
               _courseProgress[courseId].CompletedLessons.Contains(lessonId);
    }
    
    public bool HasCompletedCourse(Guid courseId)
    {
        return _courseProgress.ContainsKey(courseId) && 
               _courseProgress[courseId].IsCompleted;
    }
    
    public int GetCompletedLessonsCount(Guid courseId)
    {
        return _courseProgress.ContainsKey(courseId)
            ? _courseProgress[courseId].CompletedLessons.Count
            : 0;
    }
}

public class CourseProgress
{
    private readonly HashSet<Guid> _completedLessons = new();
    
    public Guid CourseId { get; }
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