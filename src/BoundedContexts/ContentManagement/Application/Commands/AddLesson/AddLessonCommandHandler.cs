using MediatR;
using FluencyHub.ContentManagement.Application.Common.Interfaces;
using FluencyHub.ContentManagement.Application.Common.Exceptions;
using FluencyHub.ContentManagement.Domain;
using Microsoft.Extensions.Logging;
using ICourseRepository = FluencyHub.ContentManagement.Application.Common.Interfaces.ICourseRepository;

namespace FluencyHub.ContentManagement.Application.Commands.AddLesson;

public class AddLessonCommandHandler : IRequestHandler<AddLessonCommand, Guid>
{
    private readonly ICourseRepository _courseRepository;
    private readonly ILogger<AddLessonCommandHandler> _logger;

    public AddLessonCommandHandler(ICourseRepository courseRepository, ILogger<AddLessonCommandHandler> logger)
    {
        _courseRepository = courseRepository;
        _logger = logger;
    }

    public async Task<Guid> Handle(AddLessonCommand request, CancellationToken cancellationToken)
    {
        // Verificar se o curso existe
        var course = await _courseRepository.GetByIdAsync(request.CourseId);
        if (course == null)
        {
            throw new NotFoundException($"Curso com ID {request.CourseId} não encontrado");
        }

        // Criar nova lição
        var lesson = new Lesson(
            title: request.Title,
            description: request.Description,
            content: request.Content,
            duration: request.Duration,
            order: request.Order,
            courseId: request.CourseId)
        {
            VideoUrl = request.VideoUrl
        };

        // Adicionar lição ao curso
        course.AddLesson(lesson);

        // Salvar alterações
        await _courseRepository.SaveChangesAsync(cancellationToken);

        return lesson.Id;
    }
} 