using System.ComponentModel.DataAnnotations;
using FluencyHub.Application.StudentManagement.Commands.EnrollStudent;

namespace FluencyHub.API.Models;

public class EnrollmentCreateRequest
{
    [Required]
    public Guid CourseId { get; set; }
    
    [Required]
    public Guid StudentId { get; set; }
    
    public EnrollStudentCommand ToCommand()
    {
        return new EnrollStudentCommand(StudentId, CourseId);
    }
} 