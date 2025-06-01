using FluencyHub.SharedKernel.Events;

namespace FluencyHub.StudentManagement.Domain.Events;

public class StudentEnrolledEvent : DomainEventBase
{
    public Guid StudentId { get; }
    public Guid CourseId { get; }
    public Guid EnrollmentId { get; }

    public StudentEnrolledEvent(Guid studentId, Guid courseId, Guid enrollmentId)
    {
        StudentId = studentId;
        CourseId = courseId;
        EnrollmentId = enrollmentId;
    }
}

public class StudentDeactivatedEvent : DomainEventBase
{
    public Guid StudentId { get; }

    public StudentDeactivatedEvent(Guid studentId)
    {
        StudentId = studentId;
    }
}

public class StudentActivatedEvent : DomainEventBase
{
    public Guid StudentId { get; }

    public StudentActivatedEvent(Guid studentId)
    {
        StudentId = studentId;
    }
}

public class CertificateIssuedEvent : DomainEventBase
{
    public Guid StudentId { get; }
    public Guid CourseId { get; }
    public Guid CertificateId { get; }

    public CertificateIssuedEvent(Guid studentId, Guid courseId, Guid certificateId)
    {
        StudentId = studentId;
        CourseId = courseId;
        CertificateId = certificateId;
    }
} 