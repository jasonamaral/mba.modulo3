using FluencyHub.Application.Common.Exceptions;
using FluencyHub.ContentManagement.Application.Commands.AddLesson;
using FluencyHub.ContentManagement.Application.Commands.CreateCourse;
using FluencyHub.ContentManagement.Application.Commands.DeleteCourse;
using FluencyHub.ContentManagement.Application.Commands.DeleteLesson;
using FluencyHub.ContentManagement.Application.Commands.UpdateCourse;
using FluencyHub.ContentManagement.Application.Commands.UpdateLesson;
using FluencyHub.ContentManagement.Application.Queries.GetCourseById;
using FluencyHub.ContentManagement.Application.Queries.GetLessonById;

namespace FluencyHub.Application.ContentManagement;

// Esta classe serve apenas para reexportar tipos do namespace FluencyHub.ContentManagement.Application
// para que eles possam ser usados através do namespace FluencyHub.Application.ContentManagement
public static class ContentManagementExports
{
    // Commands
    public record AddLessonCommand : FluencyHub.ContentManagement.Application.Commands.AddLesson.AddLessonCommand { }
    public record CreateCourseCommand : FluencyHub.ContentManagement.Application.Commands.CreateCourse.CreateCourseCommand { }
    public record DeleteCourseCommand : FluencyHub.ContentManagement.Application.Commands.DeleteCourse.DeleteCourseCommand { }
    public record DeleteLessonCommand : FluencyHub.ContentManagement.Application.Commands.DeleteLesson.DeleteLessonCommand { }
    public record UpdateCourseCommand : FluencyHub.ContentManagement.Application.Commands.UpdateCourse.UpdateCourseCommand { }
    public record UpdateLessonCommand : FluencyHub.ContentManagement.Application.Commands.UpdateLesson.UpdateLessonCommand { }

    // Queries and DTOs
    public class CourseDto : FluencyHub.ContentManagement.Application.Queries.GetCourseById.CourseDto { }
    public class LessonDto : FluencyHub.ContentManagement.Application.Queries.GetLessonById.LessonDto { }

    // Command handlers and other types are automatically resolved through dependency injection
} 