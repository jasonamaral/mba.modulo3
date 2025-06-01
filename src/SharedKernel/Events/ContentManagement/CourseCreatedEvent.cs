using FluencyHub.SharedKernel.Enums;

namespace FluencyHub.SharedKernel.Events.ContentManagement;

/// <summary>
/// Evento disparado quando um novo curso é criado
/// </summary>
public class CourseCreatedEvent : DomainEventBase
{
    public Guid CourseId { get; }
    public string Name { get; }
    public string Description { get; }
    public string Syllabus { get; }
    public string LearningObjectives { get; }
    public string PreRequisites { get; }
    public string TargetAudience { get; }
    public string Language { get; }
    public string Level { get; }
    public decimal Price { get; }
    public bool IsActive { get; }
    public CourseStatus Status { get; }

    public CourseCreatedEvent(
        Guid courseId, 
        string name, 
        string description,
        string syllabus,
        string learningObjectives,
        string preRequisites,
        string targetAudience,
        string language,
        string level,
        decimal price,
        bool isActive,
        CourseStatus status)
    {
        CourseId = courseId;
        Name = name;
        Description = description;
        Syllabus = syllabus;
        LearningObjectives = learningObjectives;
        PreRequisites = preRequisites;
        TargetAudience = targetAudience;
        Language = language;
        Level = level;
        Price = price;
        IsActive = isActive;
        Status = status;
    }
} 