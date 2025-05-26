using MediatR;
using FluencyHub.ContentManagement.Domain;
using FluencyHub.ContentManagement.Application.Common.Exceptions;
using Microsoft.Extensions.Logging;
using ICourseRepository = FluencyHub.ContentManagement.Application.Common.Interfaces.ICourseRepository;
using ILessonRepository = FluencyHub.ContentManagement.Application.Common.Interfaces.ILessonRepository;

namespace FluencyHub.ContentManagement.Application.Commands.AddLesson;

public class AddLessonCommandHandler : IRequestHandler<AddLessonCommand, Guid>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ILogger<AddLessonCommandHandler> _logger;

    public AddLessonCommandHandler(
        ILessonRepository lessonRepository,
        ICourseRepository courseRepository,
        ILogger<AddLessonCommandHandler> logger)
    {
        _lessonRepository = lessonRepository;
        _courseRepository = courseRepository;
        _logger = logger;
    }

    public async Task<Guid> Handle(AddLessonCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new lesson with title: {Title} for course: {CourseId}", 
            request.Title, request.CourseId);

        // Verificar se o curso existe
        var course = await _courseRepository.GetByIdAsync(request.CourseId);
        if (course == null)
        {
            _logger.LogWarning("Course with ID {CourseId} not found", request.CourseId);
            throw new NotFoundException($"Course with ID {request.CourseId} not found");
        }

        // Criar a nova lição
        var lesson = new Lesson(
            request.Title,
            request.Content,
            request.Description,
            course,
            request.Order,
            request.DurationMinutes);

        if (!string.IsNullOrEmpty(request.VideoUrl))
        {
            lesson.UpdateMaterialUrl(request.VideoUrl);
        }

        // Adicionar ao repositório
        await _lessonRepository.AddAsync(lesson, cancellationToken);
        await _lessonRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Lesson created successfully with ID: {LessonId}", lesson.Id);

        return lesson.Id;
    }
} 