namespace FluencyHub.StudentManagement.Domain;

public class LearningRecord
{
    public Guid LessonId { get; }
    public DateTime CompletedAt { get; }
    public float? Grade { get; }

    private LearningRecord() 
    { 
        // For EF Core
    }

    public LearningRecord(Guid lessonId, DateTime completedAt, float? grade = null)
    {
        LessonId = lessonId;
        CompletedAt = completedAt;
        Grade = grade;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not LearningRecord other)
            return false;

        return LessonId == other.LessonId;
    }

    public override int GetHashCode()
    {
        return LessonId.GetHashCode();
    }
} 