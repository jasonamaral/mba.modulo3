using MediatR;
using FluencyHub.ContentManagement.Application.Common.Exceptions;
using FluencyHub.ContentManagement.Application.Common.Interfaces;
using FluencyHub.ContentManagement.Domain;
using ICourseRepository = FluencyHub.ContentManagement.Application.Common.Interfaces.ICourseRepository;

namespace FluencyHub.ContentManagement.Application.Commands.AddLesson;

public class AddLessonCommandHandler : IRequestHandler<AddLessonCommand, Guid>
{
    private readonly ICourseRepository _courseRepository;
    private readonly ILessonRepository _lessonRepository;

    public AddLessonCommandHandler(
        ICourseRepository courseRepository, 
        ILessonRepository lessonRepository)
    {
        _courseRepository = courseRepository;
        _lessonRepository = lessonRepository;
    }

    public async Task<Guid> Handle(AddLessonCommand request, CancellationToken cancellationToken)
    {
        try
        {
            
            var course = await _courseRepository.GetByIdAsync(request.CourseId) ?? throw new NotFoundException($"Curso com ID {request.CourseId} não encontrado");

            var lesson = new Lesson(
                title: request.Title,
                content: request.Content,
                description: request.Description,
                course: course,
                order: request.Order,
                durationMinutes: request.DurationMinutes);


            if (!string.IsNullOrEmpty(request.VideoUrl))
            {
                lesson.UpdateMaterialUrl(request.VideoUrl);
            }

            await _lessonRepository.AddAsync(lesson, cancellationToken);
            await _lessonRepository.SaveChangesAsync(cancellationToken);

            return lesson.Id;
        }
        catch (Exception)
        {
            throw;
        }
    }
} 