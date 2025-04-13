using FluencyHub.Domain.Common;
using System.Text.Json.Serialization;

namespace FluencyHub.Domain.StudentManagement;

public class LearningHistory : BaseEntity
{
    private readonly List<CourseProgress> _courseProgress = new();
    
    public IReadOnlyCollection<CourseProgress> CourseProgress => _courseProgress;
    
    public Guid StudentId => Id;
    
    // Construtor para o EF Core
    protected LearningHistory() { }
    
    public LearningHistory(Guid studentId)
    {
        Id = studentId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void AddCourseProgress(CourseProgress progress)
    {
        _courseProgress.Add(progress);
    }
    
    public void AddProgress(Guid courseId, Guid lessonId)
    {
        var courseProgress = _courseProgress.FirstOrDefault(cp => cp.CourseId == courseId);
        if (courseProgress == null)
        {
            courseProgress = new CourseProgress(courseId) 
            {
                LearningHistoryId = this.Id
            };
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
            courseProgress = new CourseProgress(courseId) 
            {
                LearningHistoryId = this.Id
            };
            _courseProgress.Add(courseProgress);
        }
        
        courseProgress.CompleteCourse();
        UpdatedAt = DateTime.UtcNow;
    }
    
    public bool HasCompletedLesson(Guid courseId, Guid lessonId)
    {
        var courseProgress = _courseProgress.FirstOrDefault(cp => cp.CourseId == courseId);
        return courseProgress != null && courseProgress.HasCompletedLesson(lessonId);
    }
    
    public bool HasCompletedCourse(Guid courseId)
    {
        var courseProgress = _courseProgress.FirstOrDefault(cp => cp.CourseId == courseId);
        return courseProgress != null && courseProgress.IsCompleted;
    }
    
    public int GetCompletedLessonsCount(Guid courseId)
    {
        var courseProgress = _courseProgress.FirstOrDefault(cp => cp.CourseId == courseId);
        return courseProgress != null ? courseProgress.GetCompletedLessonsCount() : 0;
    }
}

public class CourseProgress
{
    private readonly List<CompletedLesson> _completedLessons = new();
    
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid CourseId { get; private set; }
    public Guid LearningHistoryId { get; set; }
    public bool IsCompleted { get; private set; }
    public DateTime LastUpdated { get; private set; }
    
    public IReadOnlyCollection<CompletedLesson> CompletedLessons => _completedLessons;
    
    // Construtor para o EF Core
    protected CourseProgress() { }
    
    public CourseProgress(Guid courseId)
    {
        CourseId = courseId;
        IsCompleted = false;
        LastUpdated = DateTime.UtcNow;
    }
    
    public void AddCompletedLesson(Guid lessonId)
    {
        if (!HasCompletedLesson(lessonId))
        {
            _completedLessons.Add(new CompletedLesson 
            { 
                LessonId = lessonId,
                CourseProgressId = Id,
                CompletedAt = DateTime.UtcNow
            });
            LastUpdated = DateTime.UtcNow;
        }
    }
    
    public bool HasCompletedLesson(Guid lessonId)
    {
        return _completedLessons.Any(cl => cl.LessonId == lessonId);
    }
    
    public int GetCompletedLessonsCount()
    {
        return _completedLessons.Count;
    }
    
    public void CompleteCourse()
    {
        IsCompleted = true;
        LastUpdated = DateTime.UtcNow;
    }
}

public class CompletedLesson
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LessonId { get; set; }
    public Guid CourseProgressId { get; set; }
    public DateTime CompletedAt { get; set; }
    
    // Relação de navegação
    public CourseProgress CourseProgress { get; set; }
} 