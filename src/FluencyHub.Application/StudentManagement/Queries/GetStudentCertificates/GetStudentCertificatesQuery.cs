using FluencyHub.Application.StudentManagement.Queries.GetCertificateById;
using MediatR;

namespace FluencyHub.Application.StudentManagement.Queries.GetStudentCertificates;

public record GetStudentCertificatesQuery(Guid StudentId) : IRequest<IEnumerable<CertificateDto>>; 