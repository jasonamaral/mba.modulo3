using FluencyHub.ContentManagement.Domain.Common;
using FluencyHub.ContentManagement.Domain.Events;
using FluencyHub.SharedKernel.Enums;
using System.Text.Json.Serialization;

namespace FluencyHub.ContentManagement.Domain;

public class Course : BaseEntity
{
    [JsonIgnore]
    private readonly List<Lesson> _lessons = [];

    public required string Name { get; set; }
    public required string Description { get; set; }
    public required CourseContent Content { get; set; }
    public decimal Price { get; private set; }
    public bool IsActive { get; private set; }
    public CourseStatus Status { get; private set; } = CourseStatus.Draft;
    public DateTime? PublishedAt { get; private set; }
    public int EnrollmentCount { get; private set; } = 0;

    [JsonIgnore]
    public IReadOnlyCollection<Lesson> Lessons => _lessons.AsReadOnly();

    // EF Core constructor
    private Course()
    {
        Name = string.Empty;
        Description = string.Empty;
        Content = null!; // Será definido pelo EF Core durante a hidratação
    }

    public Course(string name, string description, CourseContent content, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("O nome do curso não pode estar vazio", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("A descrição não pode estar vazia", nameof(description));

        if (price < 0)
            throw new ArgumentException("O preço não pode ser negativo", nameof(price));

        Name = name;
        Description = description;
        Content = content ?? throw new ArgumentException("O conteúdo do curso não pode ser nulo", nameof(content));
        Price = price;
        IsActive = true;
        Status = CourseStatus.Draft;
        CreatedAt = DateTime.UtcNow;
        
        // Adicionar evento de domínio para curso criado
        AddDomainEvent(new CourseCreatedDomainEvent(this));
    }

    public void UpdateDetails(string name, string description, CourseContent content, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("O nome do curso não pode estar vazio", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("A descrição não pode estar vazia", nameof(description));

        if (price < 0)
            throw new ArgumentException("O preço não pode ser negativo", nameof(price));

        Name = name;
        Description = description;
        Content = content ?? throw new ArgumentException("O conteúdo do curso não pode ser nulo", nameof(content));
        Price = price;
        UpdatedAt = DateTime.UtcNow;
        
        // Adicionar evento de domínio para curso atualizado
        AddDomainEvent(new CourseUpdatedDomainEvent(this));
    }

    public Lesson AddLesson(string title, string content, string description, int order, int durationMinutes = 0)
    {
        if (!IsActive)
            throw new InvalidOperationException("Não é possível adicionar lições a um curso inativo");

        var lesson = new Lesson(title, content, description, this, order, durationMinutes);
        _lessons.Add(lesson);
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CourseUpdatedDomainEvent(this));

        return lesson;
    }

    public void UpdateLesson(Guid lessonId, string title, string description, string content, string? materialUrl = null, int durationMinutes = 0)
    {
        var lesson = _lessons.FirstOrDefault(l => l.Id == lessonId) ?? throw new ArgumentException($"Lição com ID {lessonId} não encontrada", nameof(lessonId));
        lesson.Update(title, description, content, materialUrl, durationMinutes);
        UpdatedAt = DateTime.UtcNow;
        
        // Adicionar evento de domínio para curso atualizado
        AddDomainEvent(new CourseUpdatedDomainEvent(this));
    }

    public void RemoveLesson(Guid lessonId)
    {
        var lesson = _lessons.FirstOrDefault(l => l.Id == lessonId) ?? throw new ArgumentException($"Lição com ID {lessonId} não encontrada", nameof(lessonId));
        _lessons.Remove(lesson);

        var orderedLessons = _lessons.OrderBy(l => l.Order).ToList();
        for (int i = 0; i < orderedLessons.Count; i++)
        {
            orderedLessons[i].UpdateOrder(i + 1);
        }

        UpdatedAt = DateTime.UtcNow;
        
        // Adicionar evento de domínio para curso atualizado
        AddDomainEvent(new CourseUpdatedDomainEvent(this));
    }

    public void ReorderLesson(Guid lessonId, int newOrder)
    {
        if (newOrder <= 0)
            throw new ArgumentException("A ordem deve ser positiva", nameof(newOrder));

        var lesson = _lessons.FirstOrDefault(l => l.Id == lessonId) ?? throw new ArgumentException($"Lição com ID {lessonId} não encontrada", nameof(lessonId));
        var maxOrder = _lessons.Count;
        if (newOrder > maxOrder)
            newOrder = maxOrder;

        var currentOrder = lesson.Order;

        if (currentOrder == newOrder)
            return;

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

        lesson.UpdateOrder(newOrder);
        UpdatedAt = DateTime.UtcNow;
        
        // Adicionar evento de domínio para curso atualizado
        AddDomainEvent(new CourseUpdatedDomainEvent(this));
    }

    public void PublishCourse()
    {
        if (Status == CourseStatus.Published)
            return;

        Status = CourseStatus.Published;
        PublishedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        // Adicionar evento de domínio específico para curso publicado
        AddDomainEvent(new CoursePublishedDomainEvent(this));
    }

    public void ArchiveCourse()
    {
        if (Status == CourseStatus.Archived)
            return;

        Status = CourseStatus.Archived;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        
        // Adicionar evento de domínio específico para curso arquivado
        AddDomainEvent(new CourseArchivedDomainEvent(this));
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        
        // Adicionar evento de domínio específico para curso ativado
        AddDomainEvent(new CourseActivatedDomainEvent(this));
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        
        // Adicionar evento de domínio específico para curso desativado
        AddDomainEvent(new CourseDeactivatedDomainEvent(this));
    }
    
    public void IncrementEnrollmentCount()
    {
        EnrollmentCount++;
        UpdatedAt = DateTime.UtcNow;
        
        // Adicionar evento de domínio para curso atualizado
        AddDomainEvent(new CourseUpdatedDomainEvent(this));
    }
    
    public void Delete()
    {
        if (!IsActive)
            return;

        IsActive = false;
        Status = CourseStatus.Archived;
        UpdatedAt = DateTime.UtcNow;
        
        // Adicionar evento de domínio para curso deletado
        AddDomainEvent(new CourseDeletedDomainEvent(this));
    }
} 