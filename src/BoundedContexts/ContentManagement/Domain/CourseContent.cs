namespace FluencyHub.ContentManagement.Domain;

public class CourseContent
{
    public string Syllabus { get; private set; } = string.Empty;
    public string LearningObjectives { get; private set; } = string.Empty;
    public string PreRequisites { get; private set; } = string.Empty;
    public string TargetAudience { get; private set; } = string.Empty;
    public string Language { get; private set; } = string.Empty;
    public string Level { get; private set; } = string.Empty;

    // EF Core constructor
    private CourseContent() { }

    public CourseContent(
        string syllabus, 
        string learningObjectives, 
        string preRequisites, 
        string targetAudience, 
        string language, 
        string level)
    {
        if (string.IsNullOrWhiteSpace(syllabus))
            throw new ArgumentException("Syllabus cannot be empty", nameof(syllabus));
            
        if (string.IsNullOrWhiteSpace(learningObjectives))
            throw new ArgumentException("Learning objectives cannot be empty", nameof(learningObjectives));
            
        if (string.IsNullOrWhiteSpace(targetAudience))
            throw new ArgumentException("Target audience cannot be empty", nameof(targetAudience));
            
        if (string.IsNullOrWhiteSpace(language))
            throw new ArgumentException("Language cannot be empty", nameof(language));
            
        if (string.IsNullOrWhiteSpace(level))
            throw new ArgumentException("Level cannot be empty", nameof(level));
        
        Syllabus = syllabus;
        LearningObjectives = learningObjectives;
        PreRequisites = preRequisites ?? string.Empty;
        TargetAudience = targetAudience;
        Language = language;
        Level = level;
    }

    public static CourseContent Create(
        string syllabus,
        string learningObjectives,
        string preRequisites,
        string targetAudience,
        string language,
        string level)
    {
        return new CourseContent(
            syllabus,
            learningObjectives,
            preRequisites,
            targetAudience,
            language,
            level);
    }

    public CourseContent Update(
        string syllabus,
        string learningObjectives,
        string preRequisites,
        string targetAudience,
        string language,
        string level)
    {
        return new CourseContent(
            syllabus,
            learningObjectives,
            preRequisites,
            targetAudience,
            language,
            level);
    }
} 