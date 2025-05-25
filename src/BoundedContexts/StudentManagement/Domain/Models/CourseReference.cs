using FluencyHub.SharedKernel.Contracts;

namespace FluencyHub.StudentManagement.Domain.Models;

public class CourseReference : ICourse
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Language { get; private set; } = "pt-BR";
    public string Level { get; private set; } = "Iniciante";
    public decimal Price { get; private set; }
    public bool IsActive { get; private set; }

    public CourseReference(Guid id, string name, string description, decimal price, bool isActive)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
        IsActive = isActive;
    }
} 