using MediatR;

namespace FluencyHub.Application.StudentManagement.Queries.GetAllStudents;

public record GetAllStudentsQuery : IRequest<IEnumerable<StudentDto>>; 