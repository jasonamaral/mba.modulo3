using MediatR;

namespace FluencyHub.StudentManagement.Application.Queries.GetAllStudents;

public record GetAllStudentsQuery : IRequest<IEnumerable<StudentDto>>; 