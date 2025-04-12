using System.Text.Json.Serialization;
using MediatR;

namespace FluencyHub.Application.StudentManagement.Commands.EnrollStudent;

public record EnrollStudentCommand : IRequest<Guid>
{
    public Guid StudentId { get; init; }
    public Guid CourseId { get; init; }

    [JsonConstructor]
    public EnrollStudentCommand(Guid studentId, Guid courseId)
    {
        StudentId = studentId;
        CourseId = courseId;
    }
} 