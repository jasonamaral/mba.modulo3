using FluencyHub.Application.StudentManagement.Queries.GetEnrollmentById;
using MediatR;

namespace FluencyHub.Application.StudentManagement.Queries.GetStudentEnrollments;

public record GetStudentEnrollmentsQuery(Guid StudentId) : IRequest<IEnumerable<EnrollmentDto>>; 