namespace FluencyHub.StudentManagement.Domain;

public class CourseProgress
{
    private readonly List<CompletedLesson> _completedLessons = [];

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid CourseId { get; private set; }
    public Guid LearningHistoryId { get; set; }
    public bool IsCompleted { get; private set; }
    public DateTime LastUpdated { get; private set; }

    public IReadOnlyCollection<CompletedLesson> CompletedLessons => _completedLessons;

    protected CourseProgress()
    { }

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
                CompletedAt = DateTime.UtcNow,
                CourseProgress = this
            });
            LastUpdated = DateTime.UtcNow;
        }
    }
    
    public void AddCompletedLesson(CompletedLesson completedLesson)
    {
        if (!HasCompletedLesson(completedLesson.LessonId))
        {
            _completedLessons.Add(completedLesson);
            LastUpdated = DateTime.UtcNow;
        }
    }
    
    public void CompleteLesson(Guid lessonId)
    {
        AddCompletedLesson(lessonId);
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