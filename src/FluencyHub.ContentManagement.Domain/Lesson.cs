using FluencyHub.ContentManagement.Domain.Common;
using System.Text.Json.Serialization;

namespace FluencyHub.ContentManagement.Domain;

public class Lesson : BaseEntity
{
    public Guid CourseId { get; private set; }
    public string Title { get; private set; }
    public string Content { get; private set; }
    public string Description { get; private set; }
    public string? MaterialUrl { get; private set; }
    public int Order { get; private set; }
    public int DurationMinutes { get; private set; }
    public int CompletionCount { get; private set; } = 0;

    // Navegação para EF Core
    [JsonIgnore]
    public Course Course { get; private set; }

    // Construtor para EF Core
    private Lesson()
    { }

    public Lesson(string title, string description, string content, string? materialUrl, int order, int durationMinutes = 0)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("O título da aula não pode estar vazio", nameof(title));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("O conteúdo da aula não pode estar vazio", nameof(content));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("A descrição da aula não pode estar vazia", nameof(description));

        if (order <= 0)
            throw new ArgumentException("A ordem deve ser positiva", nameof(order));

        if (durationMinutes < 0)
            throw new ArgumentException("A duração não pode ser negativa", nameof(durationMinutes));

        Title = title;
        Description = description;
        Content = content;
        MaterialUrl = materialUrl;
        Order = order;
        DurationMinutes = durationMinutes;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string title, string description, string content, string? materialUrl, int durationMinutes)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("O título da aula não pode estar vazio", nameof(title));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("O conteúdo da aula não pode estar vazio", nameof(content));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("A descrição da aula não pode estar vazia", nameof(description));

        if (durationMinutes < 0)
            throw new ArgumentException("A duração não pode ser negativa", nameof(durationMinutes));

        Title = title;
        Description = description;
        Content = content;
        MaterialUrl = materialUrl;
        DurationMinutes = durationMinutes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateOrder(int newOrder)
    {
        if (newOrder <= 0)
            throw new ArgumentException("A ordem deve ser positiva", nameof(newOrder));

        Order = newOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateMaterial(string materialUrl)
    {
        MaterialUrl = materialUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDuration(int durationMinutes)
    {
        if (durationMinutes < 0)
            throw new ArgumentException("A duração não pode ser negativa", nameof(durationMinutes));

        DurationMinutes = durationMinutes;
        UpdatedAt = DateTime.UtcNow;
    }

    internal void SetCourseId(Guid courseId)
    {
        CourseId = courseId;
    }
    
    public void IncrementCompletionCount()
    {
        CompletionCount++;
        UpdatedAt = DateTime.UtcNow;
    }
} 