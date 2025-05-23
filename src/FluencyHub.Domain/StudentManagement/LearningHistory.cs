using FluencyHub.Domain.Common;

namespace FluencyHub.Domain.StudentManagement;

public class LearningHistory : BaseEntity
{
    private readonly List<CourseProgress> _courseProgress = [];
    private readonly List<LearningRecord> _records = [];

    public IReadOnlyCollection<CourseProgress> CourseProgress => _courseProgress;
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
    
    public LearningRecord GetRecord(Guid lessonId)
    {
        return _records.FirstOrDefault(r => r.LessonId == lessonId);
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