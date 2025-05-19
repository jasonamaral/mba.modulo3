using FluencyHub.ContentManagement.Domain.Common;
using System.Text.Json.Serialization;

namespace FluencyHub.ContentManagement.Domain;

public class Lesson : BaseEntity
{
    public Guid CourseId { get; private set; }
    public string Title { get; private set; }
    public string Content { get; private set; }
    public string? MaterialUrl { get; private set; }
    public int Order { get; private set; }

    // Navegação para EF Core
    [JsonIgnore]
    public Course Course { get; private set; }

    // Construtor para EF Core
    private Lesson()
    { }

    public Lesson(string title, string content, string? materialUrl, int order)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("O título da aula não pode estar vazio", nameof(title));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("O conteúdo da aula não pode estar vazio", nameof(content));

        if (order <= 0)
            throw new ArgumentException("A ordem deve ser positiva", nameof(order));

        Title = title;
        Content = content;
        MaterialUrl = materialUrl;
        Order = order;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string title, string content, string? materialUrl)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("O título da aula não pode estar vazio", nameof(title));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("O conteúdo da aula não pode estar vazio", nameof(content));

        Title = title;
        Content = content;
        MaterialUrl = materialUrl;
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

    internal void SetCourseId(Guid courseId)
    {
        CourseId = courseId;
    }
} 