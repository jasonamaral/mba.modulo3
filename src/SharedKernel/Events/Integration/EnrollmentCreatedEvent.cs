using MediatR;

namespace FluencyHub.SharedKernel.Events.Integration;

public class EnrollmentCreatedEvent : INotification
{
    public Guid EnrollmentId { get; }
    public Guid StudentId { get; }
    public Guid CourseId { get; }
    public decimal Price { get; }
    public DateTime EnrolledAt { get; }

    public EnrollmentCreatedEvent(Guid enrollmentId, Guid studentId, Guid courseId, decimal price, DateTime enrolledAt)
    {
        EnrollmentId = enrollmentId;
        StudentId = studentId;
        CourseId = courseId;
        Price = price;
        EnrolledAt = enrolledAt;
    }
} 