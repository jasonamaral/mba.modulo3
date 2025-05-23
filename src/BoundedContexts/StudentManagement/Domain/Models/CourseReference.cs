namespace FluencyHub.StudentManagement.Domain.Models;

public class CourseReference
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
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