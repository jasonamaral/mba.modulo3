using FluencyHub.Application.ContentManagement.Commands.AddLesson;
using System.ComponentModel.DataAnnotations;

namespace FluencyHub.API.Models;

public class LessonCreateRequest
{
    [Required(ErrorMessage = "Lesson title is required.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters.")]
    public required string Title { get; set; }

    [Required(ErrorMessage = "Lesson content is required.")]
    [StringLength(10000, MinimumLength = 10, ErrorMessage = "Content must be between 10 and 10,000 characters.")]
    public required string Content { get; set; }

    [Url(ErrorMessage = "Material URL must be valid.")]
    public string? MaterialUrl { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Order must be greater than or equal to 1.")]
    public int Order { get; set; } = 1;

    public AddLessonCommand ToCommand(Guid courseId)
    {
        return new AddLessonCommand
        {
            CourseId = courseId,
            Title = Title,
            Content = Content,
            MaterialUrl = MaterialUrl
        };
    }
}