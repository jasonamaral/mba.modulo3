using MediatR;
using FluencyHub.Application.StudentManagement.Queries.GetStudentById;

namespace FluencyHub.Application.StudentManagement.Queries.GetStudentByEmail
{
    public record GetStudentByEmailQuery(string Email) : IRequest<StudentDto>;
} 