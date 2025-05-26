using MediatR;

namespace FluencyHub.StudentManagement.Application.Queries.GetCertificateById;

public record GetCertificateByIdQuery : IRequest<CertificateDto>
{
    public required Guid CertificateId { get; init; }
} 