namespace FluencyHub.SharedKernel.Events.StudentManagement;

/// <summary>
/// Evento disparado quando um aluno se matricula em um curso
/// </summary>
public class StudentEnrolledEvent : DomainEventBase
{
    public Guid StudentId { get; }
    public Guid CourseId { get; }
    public Guid EnrollmentId { get; }
    public DateTime EnrollmentDate { get; }

    public StudentEnrolledEvent(Guid studentId, Guid courseId, Guid enrollmentId, DateTime enrollmentDate)
    {
        StudentId = studentId;
        CourseId = courseId;
        EnrollmentId = enrollmentId;
        EnrollmentDate = enrollmentDate;
    }
} 