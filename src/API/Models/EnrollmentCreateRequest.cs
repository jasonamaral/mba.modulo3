using System.ComponentModel.DataAnnotations;
using FluencyHub.StudentManagement.Application.Commands.EnrollStudent;

namespace FluencyHub.API.Models;

public class EnrollmentCreateRequest
{
    [Required(ErrorMessage = "Course ID is required.")]
    public Guid CourseId { get; set; }

    [Required(ErrorMessage = "Student ID is required.")]
    public Guid StudentId { get; set; }

    public decimal? DiscountPercentage { get; set; }

    public EnrollStudentCommand ToCommand()
    {
        return new EnrollStudentCommand
        {
            StudentId = StudentId,
            CourseId = CourseId,
            EnrollmentDate = DateTime.UtcNow,
            DiscountPercentage = DiscountPercentage
        };
    }
}