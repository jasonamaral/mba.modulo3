using System.ComponentModel.DataAnnotations;
using FluencyHub.Application.StudentManagement.Commands.EnrollStudent;

namespace FluencyHub.API.Models;

public class EnrollmentCreateRequest
{
    [Required(ErrorMessage = "O ID do curso é obrigatório.")]
    public Guid CourseId { get; set; }

    [Required(ErrorMessage = "O ID do estudante é obrigatório.")]
    public Guid StudentId { get; set; }

    public EnrollStudentCommand ToCommand()
    {
        return new EnrollStudentCommand(StudentId, CourseId);
    }
}