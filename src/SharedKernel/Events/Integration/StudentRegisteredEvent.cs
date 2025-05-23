using MediatR;

namespace FluencyHub.SharedKernel.Events.Integration;

public class StudentRegisteredEvent : INotification
{
    public Guid StudentId { get; }
    public string Name { get; }
    public string Email { get; }
    public DateTime RegisteredAt { get; }

    public StudentRegisteredEvent(Guid studentId, string name, string email, DateTime registeredAt)
    {
        StudentId = studentId;
        Name = name;
        Email = email;
        RegisteredAt = registeredAt;
    }
} 