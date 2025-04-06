using FluencyHub.Application.ContentManagement.Commands.AddLesson;
using System.ComponentModel.DataAnnotations;

namespace FluencyHub.API.Models;

public class LessonCreateRequest
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; }

    [Required]
    [StringLength(10000, MinimumLength = 10)]
    public string Content { get; set; }

    public string? MaterialUrl { get; set; }

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