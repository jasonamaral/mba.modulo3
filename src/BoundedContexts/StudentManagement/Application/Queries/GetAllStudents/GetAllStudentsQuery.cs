using MediatR;
using FluencyHub.StudentManagement.Application.Common.Models;

namespace FluencyHub.StudentManagement.Application.Queries.GetAllStudents;

public record GetAllStudentsQuery : IRequest<IEnumerable<StudentDto>>; 