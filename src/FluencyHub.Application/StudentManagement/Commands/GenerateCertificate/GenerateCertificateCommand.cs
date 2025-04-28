using MediatR;

namespace FluencyHub.Application.StudentManagement.Commands.GenerateCertificate;

public record GenerateCertificateCommand : IRequest<Guid>
{
    public Guid StudentId { get; init; }
    public Guid CourseId { get; init; }
} 