using FluencyHub.ContentManagement.Domain;
using MediatR;

namespace FluencyHub.ContentManagement.Application.Commands.CreateCourse;

public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, Guid>
{
    private readonly FluencyHub.ContentManagement.Application.Common.Interfaces.ICourseRepository _courseRepository;

    public CreateCourseCommandHandler(
        FluencyHub.ContentManagement.Application.Common.Interfaces.ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }

    public async Task<Guid> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {


        var courseContent = new CourseContent(
            request.Syllabus,
            request.LearningObjectives,
            request.PreRequisites,
            request.TargetAudience,
            request.Language,
            request.Level);

        var course = new Course(
            request.Name,
            request.Description,
            courseContent,
            request.Price)
        {
            Name = request.Name,
            Description = request.Description,
            Content = courseContent
        };

        await _courseRepository.AddAsync(course);
        await _courseRepository.SaveChangesAsync(cancellationToken);



        return course.Id;
    }
} 