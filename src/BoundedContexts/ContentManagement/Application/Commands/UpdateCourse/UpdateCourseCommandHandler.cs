using FluencyHub.ContentManagement.Domain;
using FluencyHub.SharedKernel.Common.Exceptions;
using MediatR;
using ICourseRepository = FluencyHub.ContentManagement.Application.Common.Interfaces.ICourseRepository;

namespace FluencyHub.ContentManagement.Application.Commands.UpdateCourse;

public class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand, bool>
{
    private readonly ICourseRepository _courseRepository;

    public UpdateCourseCommandHandler(
        ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }

    public async Task<bool> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetByIdAsync(request.Id);
        if (course == null)
        {
            throw new NotFoundException($"Curso com ID {request.Id} não encontrado");
        }

        var updatedContent = new CourseContent(
            request.Syllabus,
            request.LearningObjectives,
            request.PreRequisites,
            request.TargetAudience,
            request.Language,
            request.Level);

        course.UpdateDetails(
            request.Name,
            request.Description,
            updatedContent,
            request.Price);

        await _courseRepository.UpdateAsync(course);
        await _courseRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
} 