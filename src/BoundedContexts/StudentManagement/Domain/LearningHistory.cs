using FluencyHub.StudentManagement.Domain.Common;

namespace FluencyHub.StudentManagement.Domain;

public class LearningHistory : BaseEntity
{
    private readonly List<CourseProgress> _courseProgresses = [];
    private readonly List<LearningRecord> _records = [];

    public IReadOnlyCollection<CourseProgress> CourseProgresses => _courseProgresses;
    public IReadOnlyCollection<LearningRecord> Records => _records.AsReadOnly();

    public Guid StudentId => Id;

    protected LearningHistory()
    { }

    public LearningHistory(Guid studentId)
    {
        Id = studentId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddCourseProgress(CourseProgress progress)
    {
        _courseProgresses.Add(progress);
    }

    public void AddProgress(Guid courseId, Guid lessonId)
    {
        var courseProgress = _courseProgresses.FirstOrDefault(cp => cp.CourseId == courseId);
        if (courseProgress == null)
        {
            courseProgress = new CourseProgress(courseId)
            {
                LearningHistoryId = this.Id
            };
            _courseProgresses.Add(courseProgress);
        }

        courseProgress.AddCompletedLesson(lessonId);
        
        // Adiciona também um registro ao histórico de aprendizado
        AddLearningRecord(lessonId);
        
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddLearningRecord(Guid lessonId, float? grade = null)
    {
        // Verifica se o registro já existe para evitar duplicidade
        if (_records.Any(r => r.LessonId == lessonId))
            return;
            
        var record = new LearningRecord(lessonId, DateTime.UtcNow, grade);
        _records.Add(record);
        UpdatedAt = DateTime.UtcNow;
    }
    
    public LearningRecord? GetRecord(Guid lessonId)
    {
        return _records.FirstOrDefault(r => r.LessonId == lessonId);
    }

    public void CompleteCourse(Guid courseId)
    {
        var courseProgress = _courseProgresses.FirstOrDefault(cp => cp.CourseId == courseId);
        if (courseProgress == null)
        {
            courseProgress = new CourseProgress(courseId)
            {
                LearningHistoryId = this.Id
            };
            _courseProgresses.Add(courseProgress);
        }

        courseProgress.CompleteCourse();
        UpdatedAt = DateTime.UtcNow;
    }

    public bool HasCompletedLesson(Guid courseId, Guid lessonId)
    {
        var courseProgress = _courseProgresses.FirstOrDefault(cp => cp.CourseId == courseId);
        return courseProgress != null && courseProgress.HasCompletedLesson(lessonId);
    }

    public bool HasCompletedCourse(Guid courseId)
    {
        var courseProgress = _courseProgresses.FirstOrDefault(cp => cp.CourseId == courseId);
        return courseProgress != null && courseProgress.IsCompleted;
    }

    public int GetCompletedLessonsCount(Guid courseId)
    {
        var courseProgress = _courseProgresses.FirstOrDefault(cp => cp.CourseId == courseId);
        return courseProgress != null ? courseProgress.GetCompletedLessonsCount() : 0;
    }
} 